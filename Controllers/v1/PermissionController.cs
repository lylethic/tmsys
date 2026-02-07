using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.Request;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/permissions")]
public class PermissionController : BaseApiController
{
    private readonly IPermissionRepository _permission;

    public PermissionController(
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger,
        IPermissionRepository permission) : base(mapper, httpContextAccessor, logger)
    {
        this._permission = permission;
        this._mapper = mapper;
    }

    [HttpPost]
    [RequirePermission("SYS_ADMIN")]
    public async Task<IActionResult> CreateAsync(Permission permission)
    {
        var result = await _permission.AddAsync(permission);
        return Success(result);
    }

    [HttpPatch("id")]
    [RequirePermission("SYS_ADMIN")]
    public async Task<IActionResult> UpdateAsync(Guid id, Permission permission)
    {
        var result = await _permission.UpdateAsync(id, permission);
        return Success(result);
    }

    [HttpDelete("id")]
    [RequirePermission("SYS_ADMIN")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var result = await _permission.DeleteAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("{userId}")]
    [RequirePermission("SYS_ADMIN")]
    public async Task<IActionResult> GetUserPermissionsAsync(Guid userId)
    {
        try
        {
            var result = await _permission.GetUserPermissionsAsync(userId);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("user-role/{id}")]
    [RequirePermission("SYS_ADMIN")]
    public async Task<IActionResult> GetUserRolesAsync(Guid id)
    {
        try
        {
            var result = await _permission.GetUserRolesAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("user-permissions/{id}")]
    [RequirePermission("SYS_ADMIN")]
    public async Task<IActionResult> GetUserWithRolesAndPermissionsAsync(Guid id)
    {
        try
        {
            var result = await _permission.GetUserWithRolesAndPermissionsAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet]
    [RequirePermission("SYS_ADMIN")]
    public async Task<IActionResult> GetPermissionsAsync([FromQuery] PaginationRequest request)
    {
        try
        {
            var result = await _permission.GetPermissionsAsync(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }
}
