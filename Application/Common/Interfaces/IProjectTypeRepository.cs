using System;
using server.Application.DTOs;
using server.Application.Request;
using server.Application.Search;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface IProjectTypeRepository : IRepository<ProjectType>
{
    Task<CursorPaginatedResult<ProjectTypeModel>> GetAllAsync(BaseSearch request);
}
