using server.Application.DTOs;
using server.Application.Request;
using server.Application.Request.Search;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface IProjectRepository
{
    Task<Project> AddAsync(Project entity);
    Task<bool> DeleteItemAsync(Guid id);
    Task<PaginatedResult<Project>> GetAllAsync(PaginationRequest request);
    Task<Project> GetByIdAsync(Guid id);
    Task<bool> UpdateItemAsync(Guid id, Project entity);
    Task<PaginatedResult<ProjectModel>> GetAllAsync(ProjectSearch request);
}
