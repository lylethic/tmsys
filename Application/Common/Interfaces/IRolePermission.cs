using server.Application.Request;
using server.Application.Search;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface IRolePermission : IRepository<Role_permissions>
{
    Task<CursorPaginatedResult<Role_permissions>> GetAllAsync(BaseSearch request);
    Task<IEnumerable<Role_permissions>> AddAsync(IEnumerable<Role_permissions> request);
}
