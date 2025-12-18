using System;
using server.Application.Request;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface IProgressUpdateRepository
{
    Task<ProgressUpdate> AddAsync(ProgressUpdate entity);
    Task<bool> DeleteItemAsync(Guid id);
    Task<PaginatedResult<ProgressUpdate>> GetAllAsync(PaginationRequest request);
    Task<ProgressUpdate> GetByIdAsync(Guid id);
    Task<bool> UpdateItemAsync(Guid id, ProgressUpdate entity);
}
