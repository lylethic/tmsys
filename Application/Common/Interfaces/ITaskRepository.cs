using server.Application.Models;
using server.Application.Request;
using server.Application.Request.Search;
using server.Domain.Entities;
using System;

namespace server.Application.Common.Interfaces;

public interface ITaskRepository
{
    Task<CursorPaginatedResult<Tasks>> GetAllAsync(TaskSearch request);
    Task<TaskModel> GetDetailTaskAsync(Guid id);
    Task<Tasks> GetByIdAsync(Guid id);
    Task<Tasks> AddAsync(Tasks entity);
    Task<bool> DeleteItemAsync(Guid id);
    Task<bool> UpdateItemAsync(Guid id, Tasks entity);
}
