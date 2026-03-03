using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Application.Search;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;

namespace server.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/role-menus")]
public class RoleMenuController : BaseApiController
{
    private readonly IRoleMenuRepository _repo;

    public RoleMenuController(
        IRoleMenuRepository repo,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger) : base(mapper, httpContextAccessor, logger)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] BaseSearch request)
    {
        try
        {
            var result = await _repo.GetAllAsync(request);
            return Success(result);
        }
        catch (Exception ex) { return Error(ex.Message); }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _repo.GetByIdAsync(id);
            return Success(result);
        }
        catch (Exception ex) { return Error(ex.Message); }
    }

    /// <summary>Returns all menu permissions assigned to a role.</summary>
    [HttpGet("by-role/{roleId:guid}")]
    public async Task<IActionResult> GetByRole(Guid roleId)
    {
        try
        {
            var result = await _repo.GetByRoleIdAsync(roleId);
            return Success(result);
        }
        catch (Exception ex) { return Error(ex.Message); }
    }

    /// <summary>
    /// Returns the dynamic navigation menu for a role.
    /// Only groups/items where can_view = true are included.
    /// Used by the frontend to build the sidebar.
    /// </summary>
    [HttpGet("dynamic-menu/{roleId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDynamicMenu(Guid roleId)
    {
        try
        {
            var result = await _repo.GetDynamicMenuAsync(roleId);
            return Success(result);
        }
        catch (Exception ex) { return Error(ex.Message); }
    }

    [HttpPost]
    [RequirePermission("SYS_ADMIN", "CREATE")]
    public async Task<IActionResult> Create([FromBody] RoleMenuCreateDto dto)
    {
        try
        {
            var entity = _mapper.Map<RoleMenu>(dto);
            var result = await _repo.AddAsync(entity);
            return Success(result);
        }
        catch (Exception ex) { return Error(ex.Message); }
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("SYS_ADMIN", "UPDATE")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RoleMenuUpdateDto dto)
    {
        try
        {
            var entity = _mapper.Map<RoleMenu>(dto);
            var result = await _repo.UpdateItemAsync(id, entity);
            return Success(result);
        }
        catch (Exception ex) { return Error(ex.Message); }
    }

    /// <summary>
    /// Bulk-replace all menu permissions for a role in one call.
    /// Soft-deletes existing entries then inserts new ones in a transaction.
    /// </summary>
    [HttpPut("bulk/{roleId:guid}")]
    [RequirePermission("SYS_ADMIN", "UPDATE")]
    public async Task<IActionResult> BulkUpsert(Guid roleId, [FromBody] List<RoleMenuCreateDto> permissions)
    {
        try
        {
            var result = await _repo.UpsertRoleMenusAsync(roleId, permissions);
            return Success(result);
        }
        catch (Exception ex) { return Error(ex.Message); }
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("SYS_ADMIN", "DELETE")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _repo.DeleteItemAsync(id);
            return Success(result);
        }
        catch (Exception ex) { return Error(ex.Message); }
    }
}
