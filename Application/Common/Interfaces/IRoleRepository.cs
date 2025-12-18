using server.Application.Request;
using server.Common.Interfaces;
using server.Domain.Entities;
using System;

namespace server.Application.Common.Interfaces;

public interface IRoleRepository : IRepository<Role>
{
    Task<PaginatedResult<Role>> GetAllAsync(PaginationRequest request);
}
