using Dapper;
using Medo;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.DTOs;
using server.Application.Enums;
using server.Application.Request;
using server.Common.Interfaces;
using server.Common.Models;
using server.Common.Utils;
using server.Domain.Entities;
using server.Services.Templates;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace server.Repositories;

public class AuthenticationRepository : SimpleCrudRepository<User, string>, IAuth
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _memoryCache;
    private readonly IMailService _gmailService;
    private readonly IUserRepository _userRepo;
    private readonly IOtpRepository _otpRepo;
    private readonly ILogManager _logger;
    private IConfiguration _config;

    public AuthenticationRepository(
        IDbConnection connection,
        IHttpContextAccessor httpContextAccessor,
        IMemoryCache memoryCache,
        IMailService gmailService,
        IUserRepository userRepo,
        IOtpRepository otpRepo,
        ILogManager logger,
        IConfiguration configuration
    ) : base(connection)
    {
        this._connection = connection;
        this._httpContextAccessor = httpContextAccessor;
        this._memoryCache = memoryCache;
        this._gmailService = gmailService;
        this._userRepo = userRepo;
        this._otpRepo = otpRepo;
        this._logger = logger;
        this._config = configuration;
    }

    public async Task<AuthResponse> Login(AuthRequest model)
    {
        if (_connection.State != ConnectionState.Open)
            _connection.Open();

        using var transaction = _connection.BeginTransaction();
        try
        {
            var user = await _userRepo.GetEmailAsync(model.Email);

            if (user == null)
            {
                return new AuthResponse()
                {
                    Message = "An error occurred while validating data...",
                    Errors =
                    [
                        new Error("email", "Incorrect email address"),
                    ],
                    Status = (int)AuthStatus.UnprocessableContent
                };
            }

            if (string.IsNullOrWhiteSpace(user.Password) || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                return new AuthResponse()
                {
                    Message = "An error occurred while validating data...",
                    Errors =
                    [
                        new Error("password", "Incorrect password"),
                    ],
                    Status = (int)AuthStatus.UnprocessableContent
                };
            }

            var userRolesAndPermissions = await _userRepo.GetUserRolesAndPermissionsAsync(user.Id);

            var claims = new List<Claim>
            {
                new ("user_id", user.Id.ToString()),
                new (ClaimTypes.Email, user.Email),
                new ("session_id", Uuid7.NewUuid7().ToGuid().ToString())
            };

            foreach (var role in userRolesAndPermissions.Roles)
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

            foreach (var permission in userRolesAndPermissions.Permissions)
                claims.Add(new Claim("permission", permission.Name));

            var expiryHoursStr = _config["JwtSettings:AccessTokenExpirationHours"];
            var refreshExpiryHoursStr = _config["JwtSettings:RefreshTokenExpirationMonths"];

            if (!double.TryParse(expiryHoursStr, out double expiryHours) ||
                !double.TryParse(refreshExpiryHoursStr, out double refreshExpiryHours))
            {
                return new AuthResponse(AuthStatus.InternalServerError, "Invalid token expiry configuration");
            }

            var accessToken = GenerateAccessToken(claims);
            // var tokenExpiredTime = DateTime.UtcNow.AddMonths((int)expiryMonths);
            var tokenExpiredTime = DateTime.UtcNow.AddHours((int)expiryHours);

            var accessTokenCookieName = _config["Cookie:AccessTokenCookieName"];
            var accessTokenExpiryName = _config["Cookie:AccessTokenExpiryName"];
            var refreshTokenCookieName = _config["Cookie:RefreshTokenCookieName"];
            var refreshTokenExpiryName = _config["Cookie:RefreshTokenExpiryName"];

            if (string.IsNullOrWhiteSpace(accessTokenCookieName) || string.IsNullOrWhiteSpace(accessTokenExpiryName) ||
                string.IsNullOrWhiteSpace(refreshTokenCookieName) || string.IsNullOrWhiteSpace(refreshTokenExpiryName))
            {
                return new AuthResponse(AuthStatus.InternalServerError, "Missing cookie environment variables");
            }

            SetJWTTokenCookie(accessTokenCookieName, accessTokenExpiryName, accessToken, tokenExpiredTime);

            var loginUser = new LoginDto
            {
                // Id = user.Id,
                Email = user.Email,
                Role = userRolesAndPermissions.Roles.FirstOrDefault()?.Name ?? string.Empty,
                Permissions = [.. userRolesAndPermissions.Permissions.Select(p => p.Name)]
            };

            // update last login time to users table
            await _userRepo.UpdateLoginTime(user.Id, accessToken);

            // commit
            transaction.Commit();

            return new AuthResponse(
                AuthStatus.Success,
                accessToken,
                tokenExpiredTime,
                loginUser
            );
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<string> SendResetCode(string userEmail)
    {
        try
        {
            var user = await _userRepo.GetEmailAsync(userEmail);
            if (user == null)
                return "User not found";
            else
            {
                var resetCode = ValidatorHepler.GenerateRandomNumberList(6);

                var rows = await _otpRepo.AddAsync(user.Id, resetCode);
                if (!rows)
                    return "Failed to generate reset code.";

                #region: Send Email reset code to user
                string emailBody = await EmailTemplateManager.GetPasswordResetEmailAsync(userEmail, resetCode);
                var subject = "Your Password Reset Code for Loopy";
                var emailRequest = new SendEmailRequest(userEmail, subject, emailBody);
                await _gmailService.SendEmailAsync(emailRequest);
                #endregion

                return "Reset code sent successfully. Please check your email.";
            }
        }
        catch (Exception ex)
        {
            return $"Failed to send reset code: {ex.Message}";
        }
    }

    public async Task<string> ConfirmResetPassword(ResetPasswordRequest request)
    {
        var user = await _connection.QuerySingleOrDefaultAsync<User>(
            "SELECT id FROM users WHERE LOWER(email) = LOWER(@Email)",
            new { Email = request.Email }
        );

        if (user == null)
            return "User not found";

        var existingOtp = await _otpRepo.GetOTPCodeAsync(request.Code, user.Id);
        if (existingOtp != null)
        {
            var affected = await _userRepo.SetPassword(user.Id, request.NewPassword);

            // mark otp as used
            await _otpRepo.UpdateOTPAsync(existingOtp.Id);

            if (affected)
                return "Password has been reset successfully.";
            return "Failed to update password.";
        }
        return "OTP expired or invalid.";
    }

    private string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        // Read values from appsettings.json via _config
        var secretKey = _config["JwtSettings:SecretKey"];
        var issuer = _config["JwtSettings:Issuer"];
        var audience = _config["JwtSettings:Audience"];
        var expiryHoursStr = _config["JwtSettings:AccessTokenExpirationHours"];

        if (string.IsNullOrWhiteSpace(secretKey))
            throw new Exception("Missing JwtSettings:SecretKey in configuration.");

        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
            throw new Exception("Missing JwtSettings:Issuer or JwtSettings:Audience in configuration.");

        if (!int.TryParse(expiryHoursStr, out int expiryHours))
            expiryHours = 1; // fallback default if missing or invalid

        // Create JWT token
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiryHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private void SetJWTTokenCookie(string cookieName, string cookieNameExpire, string token, DateTime expireTime)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = expireTime,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        _httpContextAccessor.HttpContext?.Response.Cookies.Append(cookieName, token, cookieOptions);

        var readableExpireOptions = new CookieOptions
        {
            HttpOnly = false,
            Expires = expireTime,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        _httpContextAccessor.HttpContext?.Response.Cookies.Append(cookieNameExpire, expireTime.ToString("o"), readableExpireOptions);
    }

    public async Task Logout()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "user_id");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            // User is not authenticated or claim is missing
            return;
        }

        await _userRepo.ClearToken(userId);

        var accessTokenCookieName = _config["Cookie:AccessTokenCookieName"];
        var accessTokenExpiryName = _config["Cookie:AccessTokenExpiryName"];

        if (!string.IsNullOrWhiteSpace(accessTokenCookieName))
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(accessTokenCookieName);
        }
        if (!string.IsNullOrWhiteSpace(accessTokenExpiryName))
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(accessTokenExpiryName);
        }
    }
}
