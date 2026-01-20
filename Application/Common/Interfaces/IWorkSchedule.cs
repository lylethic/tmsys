using System;
using server.Application.Request;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface IWorkSchedule : IRepository<Work_schedule>
{
    Task<PaginatedResult<Work_schedule>> GetAllAsync(PaginationRequest request);
}
