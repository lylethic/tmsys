using System;
using server.Application.Models;
using server.Application.Request;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface ITaskRepository
{
    Task<Tasks> AddAsync(Tasks entity);
    Task<bool> DeleteItemAsync(Guid id);
    Task<PaginatedResult<Tasks>> GetAllAsync(PaginationRequest request);
    Task<Tasks> GetByIdAsync(Guid id);
    Task<bool> UpdateItemAsync(Guid id, Tasks entity);
    Task<TaskModel> GetDetailTaskAsync(Guid id);
}
