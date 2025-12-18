using System;
using System.Data;
using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.DTOs;
using server.Application.Request;
using server.Application.Request.Search;
using server.Common.CoreConstans;
using server.Common.Exceptions;
using server.Domain.Entities;
using server.Services;

namespace server.Repositories;

public class ProjectRepository : SimpleCrudRepository<Project, Guid>, IProjectRepository
{
    private readonly IAssistantService _assistantService;
    private readonly IUserRepository _userRepository;

    public ProjectRepository(IDbConnection connection, IAssistantService assistantService, IUserRepository userRepository)
    : base(connection)
    {
        _assistantService = assistantService;
        _userRepository = userRepository;
    }

    public async Task<Project> AddAsync(Project entity)
    {
        if (entity == null)
            throw new BadRequestException("Please provide project details.");

        entity.Id = Uuid7.NewUuid7().ToGuid();
        var existingManagerId = await _userRepository.GetByIdAsync(entity.Manager_id);
        if (existingManagerId == null)
            throw new BadRequestException("Invalid manager ID.");

        entity.Created_by = Guid.Parse(_assistantService.UserId);
        if (entity.Status is null)
        {
            entity.Status = CoreConstants.ProjectStatus.Pending.ToString();
        }
        var sql = """
            INSERT INTO projects (
                id, name, description, start_date, end_date, manager_id, status, 
                created, updated, created_by, updated_by, deleted, active, project_type
            ) 
            VALUES (
                @Id, @Name, @Description, @Start_date, @End_date, @Manager_id, @Status, 
                @Created, @Updated, @Created_by, @Updated_by, @Deleted, @Active, @Project_type
            )
        """;

        try
        {
            var queryResult = await _connection.ExecuteAsync(sql, entity);

            if (queryResult > 0)
            {
                var result = await GetByIdAsync(entity.Id);
                if (result != null)
                    return result;
                throw new BadRequestException("Project created, but failed to retrieve it.");
            }
            throw new BadRequestException("Failed to insert project into the database.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<bool> DeleteItemAsync(Guid id)
    {
        try
        {
            var existingProject = await GetByIdAsync(id)
                ?? throw new NotFoundException("Project not found");
            var sql = """
                UPDATE projects 
                SET deleted = true, active = false, updated = @Updated, updated_by = @Updated_by 
                WHERE id = @Id AND deleted = false
            """;

            var parameters = new
            {
                Id = id,
                Updated = DateTime.UtcNow,
                Updated_by = Guid.Parse(_assistantService.UserId)
            };

            var result = await _connection.ExecuteAsync(sql, parameters);
            if (result > 0)
                return true;
            throw new BadRequestException("Failed to delete project.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<PaginatedResult<Project>> GetAllAsync(PaginationRequest request)
    {
        var sql = """
            SELECT * FROM projects
            WHERE deleted = false
            ORDER BY created DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM projects WHERE deleted = false;
        """;

        var parameters = new
        {
            Offset = (request.PageIndex - 1) * request.PageSize,
            PageSize = request.PageSize
        };

        try
        {
            using var multi = await _connection.QueryMultipleAsync(sql, parameters);
            var result = multi.Read<Project>().ToList();
            var totalRecords = multi.ReadSingle<int>();

            return new PaginatedResult<Project>
            {
                Data = result.Count > 0 ? result : new List<Project>(),
                TotalCount = totalRecords,
                Page = request.PageIndex,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<PaginatedResult<ProjectModel>> GetAllAsync(ProjectSearch request)
    {
        var sql = """
            SELECT * FROM get_list_of_projects(
                @page_index, 
                @page_size, 
                @search_term,
                @p_start_date,
                @p_end_date
            );
        """;
        var parameters = new
        {
            Offset = (request.PageIndex - 1) * request.PageSize,
            PageSize = request.PageSize
        };
        return await GetListWithPaginationAndFilters<ProjectSearch, ProjectModel>(
            filter: request,
            sqlQuery: sql,
            parameterMapper: filter => new
            {
                search_term = filter?.SearchTerm,
                page_index = filter != null ? filter.PageIndex : 1,
                page_size = filter != null ? filter.PageSize : 20,
                p_start_date = filter?.P_start_date,
                p_end_date = filter?.P_end_date
            });
    }

    // public async Task<Project> GetByIdAsync(Guid id)
    // {
    //     var sql = @"
    //         SELECT * FROM projects 
    //         WHERE id = @Id AND deleted = false";

    //     var result = await _connection.QuerySingleOrDefaultAsync<Project>(sql, new { Id = id })
    //         ?? throw new NotFoundException("Project not found");
    //     return result;
    // }

    public async Task<bool> UpdateItemAsync(Guid id, Project entity)
    {
        try
        {
            var existingProject = await GetByIdAsync(id)
                ?? throw new NotFoundException("Project not found");

            entity.Id = id;
            var existingManagerId = await _userRepository.GetByIdAsync(entity.Manager_id);
            if (existingManagerId == null)
                throw new BadRequestException("Invalid manager ID.");
            entity.Updated_by = Guid.Parse(_assistantService.UserId);

            var sql = """
                UPDATE projects
                SET name = @Name, 
                    description = @Description, 
                    start_date = @Start_date, 
                    end_date = @End_date, 
                    manager_id = @Manager_id, 
                    status = @Status, 
                    updated = @Updated, 
                    updated_by = @Updated_by,
                    project_type = @Project_type
                WHERE id = @Id AND deleted = false
            """;

            var result = await _connection.ExecuteAsync(sql, entity);
            if (result > 0)
                return true;
            throw new BadRequestException("Failed to update project.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }
}
