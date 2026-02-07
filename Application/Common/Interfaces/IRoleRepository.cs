using server.Application.Request;
using server.Application.Search;
using server.Common.Interfaces;
using server.Domain.Entities;
using System;

namespace server.Application.Common.Interfaces;

public interface IRoleRepository : IRepository<Role>
{
    Task<CursorPaginatedResult<Role>> GetAllAsync(BaseSearch request);
}
