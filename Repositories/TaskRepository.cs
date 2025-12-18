using System.Data;
using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.Request;
using server.Common.Exceptions;
using server.Domain.Entities;
using server.Services;

namespace server.Repositories;

public class TaskRepository : SimpleCrudRepository<Tasks, Guid>, ITaskRepository
{
    private readonly IProjectRepository _projectrepository;
    private readonly IUserRepository _userRepository;
    private readonly IAssistantService _assistantService;

    public TaskRepository(IDbConnection connection, IUserRepository userRepository, IAssistantService assistantService, IProjectRepository projectrepository) : base(connection)
    {
        _connection = connection;
        _userRepository = userRepository;
        _assistantService = assistantService;
        _projectrepository = projectrepository;
    }

    public async Task<Tasks> AddAsync(Tasks entity)
    {
        if (entity == null)
            throw new BadRequestException("Please provide task details.");

        var existingProjectId = await _projectrepository.GetByIdAsync(entity.Project_id);
        if (existingProjectId == null)
            throw new NotFoundException($"Project with ID '{entity.Project_id}' does not exist.");

        var existingUserId = await _userRepository.GetByIdAsync(entity.Assigned_to);
        if (existingUserId == null)
            throw new NotFoundException($"User with ID '{entity.Assigned_to}' does not exist.");

        entity.Id = Uuid7.NewUuid7().ToGuid();
        entity.Created_by = Guid.Parse(_assistantService.UserId);

        var sql = """
            INSERT INTO tasks (
                id, project_id, name, description, assigned_to, status, 
                due_date, priority, created, updated, created_by, updated_by, 
                deleted, active,
                update_frequency_days, 
                last_progress_update
            ) 
            VALUES (
                @Id, @Project_id, @Name, @Description, @Assigned_to, @Status, 
                @Due_date, @Priority, @Created, @Updated, @Created_by, @Updated_by, 
                @Deleted, @Active,
                @Update_frequency_days, 
                @Last_progress_update
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
                throw new BadRequestException("Task created, but failed to retrieve it.");
            }
            throw new BadRequestException("Failed to insert task into the database.");
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
            var existingTask = await GetByIdAsync(id)
                ?? throw new NotFoundException("Task not found");

            var sql = @"
                UPDATE tasks 
                SET deleted = true, active = false, updated = @Updated, updated_by = @Updated_by 
                WHERE id = @Id AND deleted = false";

            var parameters = new
            {
                Id = id,
                Updated_by = Guid.Parse(_assistantService.UserId)
            };

            var result = await _connection.ExecuteAsync(sql, parameters);
            if (result > 0)
                return true;
            throw new BadRequestException("Failed to delete task.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<PaginatedResult<Tasks>> GetAllAsync(PaginationRequest request)
    {
        var sql = @"
            SELECT * FROM tasks
            WHERE deleted = false
            ORDER BY created DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM tasks WHERE deleted = false;
        ";

        var parameters = new
        {
            Offset = (request.PageIndex - 1) * request.PageSize,
            PageSize = request.PageSize
        };

        try
        {
            using var multi = await _connection.QueryMultipleAsync(sql, parameters);
            var result = multi.Read<Tasks>().ToList();
            var totalRecords = multi.ReadSingle<int>();

            return new PaginatedResult<Tasks>
            {
                Data = result.Count > 0 ? result : new List<Tasks>(),
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

    public override async Task<Tasks> GetByIdAsync(Guid id)
    {
        var sql = """
            SELECT * FROM tasks 
            WHERE id = @Id AND deleted = false
        """;

        var result = await _connection.QuerySingleOrDefaultAsync<Tasks>(sql, new { Id = id })
            ?? throw new NotFoundException("Task not found");
        return result;
    }

    public async Task<bool> UpdateItemAsync(Guid id, Tasks entity)
    {
        try
        {
            var existingTask = await GetByIdAsync(id)
                ?? throw new NotFoundException("Task not found");

            entity.Id = id;

            var existingProjectId = await _projectrepository.GetByIdAsync(entity.Project_id);
            if (existingProjectId == null)
                throw new NotFoundException($"Project with ID '{entity.Project_id}' does not exist.");

            var existingUserId = await _userRepository.GetByIdAsync(entity.Assigned_to);
            if (existingUserId == null)
                throw new NotFoundException($"User with ID '{entity.Assigned_to}' does not exist.");

            entity.Updated_by = Guid.Parse(_assistantService.UserId);

            var sql = """
                UPDATE tasks
                SET project_id = @Project_id, 
                    name = @Name, 
                    description = @Description, 
                    assigned_to = @Assigned_to, 
                    status = @Status, 
                    due_date = @Due_date, 
                    priority = @Priority, 
                    updated = @Updated, 
                    updated_by = @Updated_by,
                    update_frequency_days = @Update_frequency_days,  
                    last_progress_update = @Last_progress_update
                WHERE id = @Id AND deleted = false
            """;

            var result = await _connection.ExecuteAsync(sql, entity);
            if (result > 0)
                return true;
            throw new BadRequestException("Failed to update task.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }
}
