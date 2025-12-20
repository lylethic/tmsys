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
using server.Domain.Entities;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace server.Repositories;

public class AuthenticationRepository : SimpleCrudRepository<User, string>, IAuth
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _memoryCache;
    private readonly IMailService _gmailService;
    private readonly IUserRepository _userRepo;
    private IConfiguration _config;

    public AuthenticationRepository(
        IDbConnection connection,
        IHttpContextAccessor httpContextAccessor,
        IMemoryCache memoryCache,
        IMailService gmailService,
        IUserRepository userRepo,
        IConfiguration configuration
    ) : base(connection)
    {
        this._connection = connection;
        this._httpContextAccessor = httpContextAccessor;
        this._memoryCache = memoryCache;
        this._gmailService = gmailService;
        this._userRepo = userRepo;
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
            var refreshToken = GenerateRefreshToken();
            var tokenExpiredTime = DateTime.UtcNow.AddHours((int)expiryHours);

            var refreshTokenExpiredTime = DateTime.UtcNow.AddMonths((int)refreshExpiryHours);
            // Hash the refresh token before storing
            var hashedRefreshToken = HashToken(refreshToken);

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
            SetJWTTokenCookie(refreshTokenCookieName, refreshTokenExpiryName, accessToken, tokenExpiredTime);

            var loginUser = new LoginDto
            {
                // Id = user.Id,
                Email = user.Email,
                Role = userRolesAndPermissions.Roles.FirstOrDefault()?.Name ?? string.Empty,
                Permissions = [.. userRolesAndPermissions.Permissions.Select(p => p.Name)]
            };

            // update last login time to users table
            await _userRepo.UpdateLoginTime(user.Id, hashedRefreshToken);

            // commit
            transaction.Commit();

            return new AuthResponse(
                AuthStatus.Success,
                accessToken,
                tokenExpiredTime,
                refreshToken,
                refreshTokenExpiredTime,
                loginUser
            );
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
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
        var refreshTokenCookieName = _config["Cookie:RefreshTokenCookieName"];
        var refreshTokenExpiryName = _config["Cookie:RefreshTokenExpiryName"];

        if (!string.IsNullOrWhiteSpace(accessTokenCookieName))
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(accessTokenCookieName);
        }
        if (!string.IsNullOrWhiteSpace(accessTokenExpiryName))
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(accessTokenExpiryName);
        }
        if (!string.IsNullOrWhiteSpace(refreshTokenCookieName))
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(refreshTokenCookieName);
        }
        if (!string.IsNullOrWhiteSpace(refreshTokenExpiryName))
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(refreshTokenExpiryName);
        }
    }

    public async Task<AuthResponse> RevokeRefreshToken(string? refreshToken)
    {
        // prefer explicit token, otherwise fall back to cookie
        var refreshTokenCookieName = _config["Cookie:RefreshTokenCookieName"];
        var tokenToRevoke = !string.IsNullOrWhiteSpace(refreshToken)
            ? refreshToken
            : _httpContextAccessor.HttpContext?.Request.Cookies[refreshTokenCookieName ?? string.Empty];

        if (string.IsNullOrWhiteSpace(tokenToRevoke))
        {
            return new AuthResponse(AuthStatus.BadRequest, "Refresh token is required");
        }

        var hashedToken = HashToken(tokenToRevoke);

        var rows = await _connection.ExecuteAsync(
            """
                UPDATE users
                SET token = NULL
                WHERE token = @Token
            """,
            new { Token = hashedToken });

        if (rows == 0)
        {
            return new AuthResponse(AuthStatus.Unauthorized, "Refresh token is invalid or already revoked");
        }

        ClearRefreshTokenCookies();
        return new AuthResponse(AuthStatus.Success, "Refresh token revoked");
    }

    public async Task<bool> UpdateRemoveToken(string email)
    {
        var update = """
            UPDATE public.users
            SET token = NULL
            WHERE email = @Email
        """;
        var affected = await _connection.ExecuteAsync(update, new { Email = email });
        if (affected > 0)
            return true;
        return false;
    }

    #region Helper method generate token
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    // Helper method to hash tokens before storing them
    private static string HashToken(string token)
    {
        //The refresh token is hashed using SHA256 before storing it in the database to prevent token theft from compromising security.
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashedBytes);
    }

    private void ClearRefreshTokenCookies()
    {
        var refreshTokenCookieName = _config["Cookie:RefreshTokenCookieName"];
        var refreshTokenExpiryName = _config["Cookie:RefreshTokenExpiryName"];

        if (!string.IsNullOrWhiteSpace(refreshTokenCookieName))
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(refreshTokenCookieName);
        }
        if (!string.IsNullOrWhiteSpace(refreshTokenExpiryName))
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(refreshTokenExpiryName);
        }
    }
    #endregion
}
