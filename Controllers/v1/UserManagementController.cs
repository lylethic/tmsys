using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using server.Common.Settings;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Domain.Entities;
using server.Common.Interfaces;
using server.Services;
using Microsoft.AspNetCore.Authorization;
using server.Application.Models;
using Asp.Versioning;
using Medo;
using server.Application.Request.Search;

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
    public async Task<IActionResult> GetMyInfo()
    {
        try
        {
            var userId = Guid.Parse(_assistantService.UserId);
            var user = await _userRepo.GetByIdAsync(userId);
            var rolesAndPermissions = await _userRepo.GetUserRolesAndPermissionsAsync(userId);

            var userInfo = new
            {
                user.Id,
                user.Username,
                user.Name,
                user.Email,
                user.ProfilePic,
                user.City,
                user.Active,
                user.Created,
                user.Updated,
                user.Last_login_time
            };

            return Success(new
            {
                User = userInfo,
                Roles = rolesAndPermissions.Roles.Select(r => r.Name).ToArray(),
                Permissions = rolesAndPermissions.Permissions.Select(p => p.Name).ToArray()
            });
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet]
    [RequirePermission("SYS_ADMIN", "READ")]
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
    [RequirePermission("SYS_ADMIN", "CREATE")]
    public async Task<IActionResult> Add([FromBody] CreateUserDto dto)
    {
        try
        {
            var request = _mapper.Map<User>(dto);
            var result = await _userRepo.RegisterUser(request);
            result.Password = null;
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpDelete("id")]
    [RequirePermission("SYS_ADMIN", "DELETE")]
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
    [RequirePermission("SYS_ADMIN", "DELETE")]
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
    [RequirePermission("SYS_ADMIN", "EDIT")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdate dto)
    {
        try
        {
            var user = _mapper.Map<User>(dto);
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
