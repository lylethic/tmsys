using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using server.Application.Request;
using server.Common.Settings;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Domain.Entities;
using System.Security.Claims;
using server.Common.Interfaces;
using server.Services;
using Microsoft.AspNetCore.Authorization;
using server.Common.CoreConstans;
using server.Application.Models;
using Asp.Versioning;
using Medo;

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/users")]
public class UserManagementController : BaseApiController
{
    private readonly IUserRepository _userRepo;
    private readonly IPermissionService _permissionService;
    private readonly IAssistantService _assistantService;
    private readonly IAuth _auth;
    public UserManagementController(
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger,
        IUserRepository userRepo,
        IPermissionService permissionService,
        IAssistantService assistantService,
        IAuth auth
        )
        : base(mapper, httpContextAccessor, logger)
    {
        this._userRepo = userRepo;
        this._mapper = mapper;
        this._permissionService = permissionService;
        this._assistantService = assistantService;
        this._auth = auth;
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        try
        {
            var result = await _userRepo.GetByIdAsync(Guid.Parse(_assistantService.UserId));
            var role = _assistantService.Role;
            var permissions = _assistantService.Permissions;
            result.Password = "";
            result.Token = "";
            var user = new
            {
                username = result.Username,
                name = result.Name,
                email = result.Email,
                city = result.City,
                profilepic = result.ProfilePic,
                last_login_time = result.Last_login_time,
                is_send_email = result.Is_send_email,
                profilepic_data = result.Profilepic_data,
                active = result.Active,
                deleted = result.Deleted
            };
            return Success(new
            {
                user,
                role,
                permissions
            });
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet]
    [RequirePermission("READ", "AM_READ")]
    public async Task<IActionResult> GetAll([FromQuery] UserSearch request)
    {
        try
        {
            var result = await _userRepo.GetAllAsync(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("id")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var data = await _userRepo.GetByIdAsync(id);
            var result = _mapper.Map<UserModel>(data);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("user_with_permission/id")]
    public async Task<IActionResult> GetUserWithPermissonById(Guid id)
    {
        try
        {
            var result = await _userRepo.GetUserWithPermissionAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [AllowAnonymous]
    [HttpPost]
    [RequirePermission("CREATE", "AM_CREATE")]
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

    [HttpDelete("id")]
    [RequirePermission("DELETE", "AM_DELETE")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _userRepo.DeleteItemAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Delete many users by their IDs.
    /// Body: ["userId1", "userId2", ..., "userIdN"]
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpDelete("bulk")]
    [RequirePermission("DELETE", "AM_DELETE")]
    public async Task<IActionResult> DeleteMany([FromBody] List<Guid> ids)
    {
        try
        {
            if (ids == null || ids.Count == 0)
                return Error("No IDs provided.");

            var result = await _userRepo.DeleteItemsAsync([.. ids]);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPatch("id")]
    [RequirePermission("EDIT", "AM_EDIT")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UserUpdate dto)
    {
        try
        {

            string imageUrl = "";

            if (dto.ProfilePic != null)
            {
                var uploadFolder = Path.Combine("wwwroot", CoreConstants.UploadFolder);
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), uploadFolder);
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                var fileName = $"{Uuid7.NewUuid7().ToGuid()}{Path.GetExtension(dto.ProfilePic.FileName)}";
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ProfilePic.CopyToAsync(stream);
                }
                var baseUrl = $"{Request.Scheme}://{Request.Host}/{CoreConstants.Prefix}";
                imageUrl = $"{baseUrl}/{CoreConstants.UploadFolder}/{fileName}";

                // Debug
                _logger.Info($"Uploaded file saved to: {filePath}");
                _logger.Info($"Public URL: {imageUrl}");
            }

            var user = _mapper.Map<User>(dto);
            if (!string.IsNullOrEmpty(imageUrl))
                user.ProfilePic = imageUrl;

            var result = await _userRepo.UpdateItemAsync(id, user);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost("{userId}/avatarUpload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAvatar([FromRoute] Guid userId, IFormFile avatar)
    {
        try
        {
            if (avatar == null)
            {
                return Error("Please select a file (avatar)");
            }
            var result = await _userRepo.UpdateAvatar(userId, avatar);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

}
