using Asp.Versioning;
using AutoMapper;
using Medo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Application.Request;
using server.Common.CoreConstans;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;
using server.Repositories;

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/auth")]
public class AuthsController : BaseApiController
{
    private readonly IAuth _auth;
    private readonly IUserRepository _userRepo;
    private readonly SeedDataService _seedDataService;
    public AuthsController(
        IServiceProvider serviceProvider,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger,
        IAuth auth, IUserRepository userRepo) : base(mapper, httpContextAccessor, logger)
    {
        _auth = auth;
        _userRepo = userRepo;
        this._seedDataService = serviceProvider.GetRequiredService<SeedDataService>();
    }

    /// <summary>
    /// Login a user with email and password.
    /// </summary>
    /// <param name="model">The login credentials.</param>
    /// <returns>An IActionResult containing the access token and refresh token on success, or an error response on failure.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(AuthRequest model)
    {
        try
        {
            var result = await _auth.Login(model);

            if (result.Status != 200)
            {
                return StatusCode(result.Status, new
                {
                    status = result.Status,
                    success = false,
                    message = result.Message,
                    errors = result.Errors
                });
            }

            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Register a new employee.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("add-employee")]
    public async Task<IActionResult> Register([FromBody] UserDto dto)
    {
        try
        {
            var imageUrl = "";
            if (dto.ProfilePic != null)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", CoreConstants.UploadFolder);
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                var fileName = $"{Uuid7.NewUuid7().ToGuid()}{Path.GetExtension(dto.ProfilePic.FileName)}";
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ProfilePic.CopyToAsync(stream);
                }

                // Build public URL
                var baseUrl = $"{Request.Scheme}://{Request.Host}/{CoreConstants.Prefix}";
                imageUrl = $"{baseUrl}/{CoreConstants.UploadFolder}/{fileName}";
            }

            var request = _mapper.Map<User>(dto);
            request.ProfilePic = imageUrl;
            var result = await _userRepo.AddAsync(request);
            if (result is not null)
            {
                return Success("User added successfully");
            }
            return Error("Failed to add user");
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Register a new user. Role User
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Add([FromForm] CreateUserDto dto)
    {
        try
        {
            var imageUrl = "";
            if (dto.ProfilePic != null)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", CoreConstants.UploadFolder);
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                var fileName = $"{Uuid7.NewUuid7().ToGuid()}{Path.GetExtension(dto.ProfilePic.FileName)}";
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ProfilePic.CopyToAsync(stream);
                }

                // Build public URL
                var baseUrl = $"{Request.Scheme}://{Request.Host}/{CoreConstants.Prefix}";
                imageUrl = $"{baseUrl}/{CoreConstants.UploadFolder}/{fileName}";
            }

            var request = _mapper.Map<User>(dto);
            request.ProfilePic = imageUrl;
            var result = await _userRepo.RegisterUser(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    // [AllowAnonymous]
    // [HttpPost("seed")]
    // public async Task<IActionResult> SeedData()
    // {
    //     try
    //     {
    //         await _seedDataService.SeedPermission();
    //         await _seedDataService.SeedRole();
    //         return Success("Seed data inserted successfully");
    //     }
    //     catch (Exception ex)
    //     {
    //         return Error(ex.Message);
    //     }
    // }

    [HttpPost("send-code")]
    public async Task<IActionResult> SendCode(string userEmail)
    {
        try
        {
            var result = await _auth.SendResetCode(userEmail);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost("confirm-reset-password")]
    public async Task<IActionResult> ConfirmResetPassword(ResetPasswordRequest request)
    {
        try
        {
            var result = await _auth.ConfirmResetPassword(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    // [HttpGet("debug/env")]
    // public IActionResult GetEnvironmentVariables()
    // {
    //     var envVars = new Dictionary<string, string?>
    //     {
    //         { "ACCESSTOKEN_COOKIENAME", Environment.GetEnvironmentVariable("ACCESSTOKEN_COOKIENAME") },
    //         { "ACCESSTOKEN_EXPIRY_COOKIENAME", Environment.GetEnvironmentVariable("ACCESSTOKEN_EXPIRY_COOKIENAME") },
    //         { "REFRESHTOKEN_COOKIENAME", Environment.GetEnvironmentVariable("REFRESHTOKEN_COOKIENAME") },
    //         { "REFRESHTOKEN_EXPIRY_COOKIENAME", Environment.GetEnvironmentVariable("REFRESHTOKEN_EXPIRY_COOKIENAME") },
    //         { "JWT_EXPIRY_HOURS", Environment.GetEnvironmentVariable("JWT_EXPIRY_HOURS") },
    //         { "JWT_REFRESH_EXPIRY_HOURS", Environment.GetEnvironmentVariable("JWT_REFRESH_EXPIRY_HOURS") },
    //         // { "API_SECRET", Environment.GetEnvironmentVariable("API_SECRET") },
    //         { "JWT_ISSUER", Environment.GetEnvironmentVariable("JWT_ISSUER") },
    //         { "JWT_AUDIENCE", Environment.GetEnvironmentVariable("JWT_AUDIENCE") }
    //     };
    //     return Ok(envVars);
    // }

    /// <summary>
    /// Logout the current user.
    /// </summary>
    /// <returns></returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _auth.Logout();
            return Success("Logged out successfully.");
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }
}