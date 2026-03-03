using server.Application.Request;
using server.Application.Search;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface ISysMenuRepository : IRepository<SysMenu>
{
    Task<CursorPaginatedResult<SysMenu>> GetAllAsync(BaseSearch request);

    /// <summary>Returns all active menus that belong to a specific group.</summary>
    Task<List<SysMenu>> GetByGroupIdAsync(Guid menuGroupId);
}
