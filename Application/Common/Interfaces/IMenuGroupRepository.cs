using server.Application.Request;
using server.Application.Search;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface IMenuGroupRepository : IRepository<MenuGroup>
{
    Task<CursorPaginatedResult<MenuGroup>> GetAllAsync(BaseSearch request);

    /// <summary>
    /// Returns full tree (groups + children + menus) for building the menu sidebar.
    /// </summary>
    Task<List<MenuGroup>> GetTreeAsync();
}
