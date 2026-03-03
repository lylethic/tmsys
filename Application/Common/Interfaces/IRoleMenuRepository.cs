using server.Application.DTOs;
using server.Application.Request;
using server.Application.Search;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface IRoleMenuRepository : IRepository<RoleMenu>
{
    Task<CursorPaginatedResult<RoleMenu>> GetAllAsync(BaseSearch request);

    /// <summary>Returns all permission records for a specific role.</summary>
    Task<List<RoleMenu>> GetByRoleIdAsync(Guid roleId);

    /// <summary>
    /// Builds the full dynamic menu tree for the given role,
    /// including only groups/menus the role has CanView = true.
    /// </summary>
    Task<List<DynamicMenuDto>> GetDynamicMenuAsync(Guid roleId);

    /// <summary>
    /// Bulk-upsert permissions for a role: replaces all role_menus rows for that role.
    /// </summary>
    Task<bool> UpsertRoleMenusAsync(Guid roleId, List<RoleMenuCreateDto> permissions);
}
