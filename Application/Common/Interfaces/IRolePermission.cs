using System;
using server.Application.Request;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface IRolePermission : IRepository<Role_permissions>
{
    public Task<IEnumerable<Role_permissions>> AddAsync(IEnumerable<Role_permissions> request);
    Task<PaginatedResult<Role_permissions>> GetAllAsync(PaginationRequest request);
}
