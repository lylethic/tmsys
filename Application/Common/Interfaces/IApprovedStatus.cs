
using server.Application.Request;
using server.Application.Request.Search;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface IApprovedStatus
{
    Task<Approved_status> AddAsync(Approved_status entity);
    Task DeleteItemAsync(Guid approvedStatusId);
    Task<Approved_status> GetByIDAsync(Guid id);
    Task<Approved_status> UpdateItemAsync(Approved_status entity);
    Task<CursorPaginatedResult<Approved_status>> GetTaskStatusPageAsync(ApprovedSearch request);
}
