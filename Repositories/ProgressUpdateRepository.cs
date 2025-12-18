using System;
using System.Data;
using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Request;
using server.Common.Exceptions;
using server.Domain.Entities;
using server.Services;

namespace server.Repositories;

public class ProgressUpdateRepository : IProgressUpdateRepository
{
    private readonly IDbConnection _connection;
    private readonly IProjectRepository _projectrepository;
    private readonly IUserRepository _userRepository;
    private readonly IAssistantService _assistantService;
    private readonly ITaskRepository _taskRepository;

    public ProgressUpdateRepository(IDbConnection connection, IUserRepository userRepository, IAssistantService assistantService, IProjectRepository projectrepository, ITaskRepository taskRepository)
    {
        _connection = connection;
        _userRepository = userRepository;
        _assistantService = assistantService;
        _projectrepository = projectrepository;
        _taskRepository = taskRepository;
    }

    public async Task<ProgressUpdate> AddAsync(ProgressUpdate entity)
    {
        if (entity == null)
            throw new BadRequestException("Please provide progress update details.");

        entity.id = Uuid7.NewUuid7().ToGuid();
        entity.User_id = Guid.Parse(_assistantService.UserId);

        var existingTaskId = await _taskRepository.GetByIdAsync(entity.Task_id);
        if (existingTaskId == null)
            throw new NotFoundException($"Task with ID '{entity.Task_id}' does not exist.");

        if (entity.Progress_percentage < 0 || entity.Progress_percentage > 100)
            throw new BadRequestException("Progress percentage must be between 0 and 100.");

        var sql = """
            INSERT INTO progress_updates (
                id, task_id, user_id, update_date, progress_percentage, notes,
                created, updated, created_by, updated_by, deleted, active
            ) 
            VALUES (
                @Id, @Task_id, @User_id, @Update_date, @Progress_percentage, @Notes,
                @Created, @Updated, @Created_by, @Updated_by, @Deleted, @Active
            )
        """;

        try
        {
            var queryResult = await _connection.ExecuteAsync(sql, entity);

            if (queryResult > 0)
            {
                var result = await GetByIdAsync(entity.id);
                if (result != null)
                    return result;
                throw new BadRequestException("Progress update created, but failed to retrieve it.");
            }
            throw new BadRequestException("Failed to insert progress update into the database.");
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
            var existingProgressUpdate = await GetByIdAsync(id)
                ?? throw new NotFoundException("Progress update not found");

            var sql = @"
                UPDATE progress_updates 
                SET deleted = true, active = false, updated = @Updated, updated_by = @Updated_by 
                WHERE id = @Id AND deleted = false";

            var parameters = new
            {
                Id = id
            };

            var result = await _connection.ExecuteAsync(sql, parameters);
            if (result > 0)
                return true;
            throw new BadRequestException("Failed to delete progress update.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<PaginatedResult<ProgressUpdate>> GetAllAsync(PaginationRequest request)
    {
        var sql = """
            SELECT * FROM progress_updates
            WHERE deleted = false
            ORDER BY created DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM progress_updates WHERE deleted = false;
        """;

        var parameters = new
        {
            Offset = (request.PageIndex - 1) * request.PageSize,
            PageSize = request.PageSize
        };

        try
        {
            using var multi = await _connection.QueryMultipleAsync(sql, parameters);
            var result = multi.Read<ProgressUpdate>().ToList();
            var totalRecords = multi.ReadSingle<int>();

            return new PaginatedResult<ProgressUpdate>
            {
                Data = result.Count > 0 ? result : new List<ProgressUpdate>(),
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

    public async Task<ProgressUpdate> GetByIdAsync(Guid id)
    {
        var sql = """
            SELECT * FROM progress_updates 
            WHERE id = @Id AND deleted = false
        """;

        var result = await _connection.QuerySingleOrDefaultAsync<ProgressUpdate>(sql, new { Id = id })
            ?? throw new NotFoundException("Progress update not found");
        return result;
    }

    public async Task<bool> UpdateItemAsync(Guid id, ProgressUpdate entity)
    {
        try
        {
            var existingProgressUpdate = await GetByIdAsync(id)
                ?? throw new NotFoundException("Progress update not found");

            entity.id = id;
            var existingTaskId = await _taskRepository.GetByIdAsync(entity.Task_id);
            if (existingTaskId == null)
                throw new NotFoundException($"Task with ID '{entity.Task_id}' does not exist.");

            if (entity.Progress_percentage < 0 || entity.Progress_percentage > 100)
                throw new BadRequestException("Progress percentage must be between 0 and 100.");

            var sql = """
                UPDATE progress_updates
                SET task_id = @Task_id, 
                    user_id = @User_id, 
                    update_date = @Update_date, 
                    progress_percentage = @Progress_percentage, 
                    notes = @Notes, 
                    updated = @Updated, 
                    updated_by = @Updated_by
                WHERE id = @Id AND deleted = false
            """;

            var result = await _connection.ExecuteAsync(sql, entity);
            if (result > 0)
                return true;
            throw new BadRequestException("Failed to update progress update.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }
}
