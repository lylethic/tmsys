using Dapper;
using Medo;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.Models;
using server.Application.Request;
using server.Application.Request.Search;
using server.Common.Exceptions;
using server.Domain.Entities;
using server.Services;
using System.Data;

namespace server.Repositories;

public class TaskRepository : SimpleCrudRepository<Tasks, Guid>, ITaskRepository
{
    private readonly IProjectRepository _projectRepo;
    private readonly IUserRepository _userRepository;
    private readonly IAssistantService _assistantService;

    public TaskRepository(IDbConnection connection,
    IUserRepository userRepository,
    IAssistantService assistantService,
    IProjectRepository projectrepository) : base(connection)
    {
        _connection = connection;
        _userRepository = userRepository;
        _assistantService = assistantService;
        _projectRepo = projectrepository;
    }

    public async Task<Tasks> AddAsync(Tasks entity)
    {
        if (entity == null)
            throw new BadRequestException("Please provide task details.");

        var existingProjectId = await _projectRepo.GetByIdAsync(entity.Project_id);
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
                last_progress_update,
                max_point,
                late_penalty,
                allow_late,
                allow_resubmit,
                pass_point,
                completed_at
            ) 
            VALUES (
                @Id, @Project_id, @Name, @Description, @Assigned_to, @Status, 
                @Due_date, @Priority, @Created, @Updated, @Created_by, @Updated_by, 
                @Deleted, @Active,
                @Update_frequency_days, 
                @Last_progress_update,
                @Max_point,
                @Late_penalty,
                @Allow_late,
                @Allow_resubmit,
                @Pass_point,
                @Completed_at
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

    public async Task<CursorPaginatedResult<Tasks>> GetAllAsync(TaskSearch request)
    {
        var where = new List<string>();
        var param = new DynamicParameters();
        where.Add("deleted = false");
        if (request.ProjectId != null)
        {
            where.Add("project_id = @ProjectId");
            param.Add("ProjectId", request.ProjectId);
        }
        if (request.AssignedTo != null)
        {
            where.Add("assigned_to = @AssignedTo");
            param.Add("AssignedTo", request.AssignedTo);
        }
        if (request.Status != null)
        {
            where.Add("status = @Status");
            param.Add("Status", request.Status);
        }
        if (request.DueDate != null)
        {
            where.Add("due_date = @DueDate");
            param.Add("DueDate", request.DueDate);
        }
        if (request.AllowLate != null)
        {
            where.Add("allow_late = @AllowLate");
            param.Add("AllowLate", request.AllowLate);
        }
        if (request.AllowResubmit != null)
        {
            where.Add("allow_resubmit = @AllowResubmit");
            param.Add("AllowResubmit", request.AllowResubmit);
        }
        if (request.PassPoint != null)
        {
            where.Add("pass_point = @PassPoint");
            param.Add("PassPoint", request.PassPoint);
        }
        if (request.CompletedAt != null)
        {
            where.Add("completed_at = @CompletedAt");
            param.Add("CompletedAt", request.CompletedAt);
        }
        var page = await this.GetListCursorBasedAsync<Tasks>(
            request: request,
            extraWhere: string.Join(" AND ", where),
            extraParams: param,
            orderByColumn: "id",
            idColumn: "id"
         );
        return page;
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

    public async Task<TaskModel> GetDetailTaskAsync(Guid id)
    {
        var sql = """
            SELECT * FROM tasks
            WHERE id = @Id AND deleted = false
        """;

        var taskResult = await _connection.QuerySingleOrDefaultAsync<Tasks>(sql, new { Id = id })
            ?? throw new NotFoundException("Task not found");

        var extendProject = await _projectRepo.GetByIdAsync(taskResult.Project_id);
        var extendUser = await _userRepository.GetByIdAsync(taskResult.Assigned_to);

        var result = new TaskModel()
        {
            Id = taskResult.Id,
            Project_id = taskResult.Project_id,
            Name = taskResult.Name,
            Description = taskResult.Description,
            Assigned_to = taskResult.Assigned_to,
            Status = taskResult.Status,
            Due_date = taskResult.Due_date,
            Priority = taskResult.Priority,
            Update_frequency_days = taskResult.Update_frequency_days,
            Last_progress_update = taskResult.Last_progress_update,
            Max_point = taskResult.Max_point,
            Late_penalty = taskResult.Late_penalty,
            Allow_late = taskResult.Allow_late,
            Allow_resubmit = taskResult.Allow_resubmit,
            Pass_point = taskResult.Pass_point,
            Completed_at = taskResult.Completed_at,
            Created = taskResult.Created,
            Updated = taskResult.Updated,
            Created_by = taskResult.Created_by,
            Updated_by = taskResult.Updated_by,
            Deleted = taskResult.Deleted,
            Active = taskResult.Active,
            Extend_project = new
            {
                name = extendProject.Name,
                description = extendProject.Description,
                start_date = extendProject.Start_date,
                end_date = extendProject.End_date,
                status = extendProject.Status,
                project_type = extendProject.Project_type
            },
            Extend_user = new
            {
                name = extendUser.Name,
                profilepic = extendUser.ProfilePic
            }
        };
        return result;
    }

    public async Task<bool> UpdateItemAsync(Guid id, Tasks entity)
    {
        try
        {
            var existingTask = await GetByIdAsync(id)
                ?? throw new NotFoundException("Task not found");

            entity.Id = id;

            var existingProjectId = await _projectRepo.GetByIdAsync(entity.Project_id);
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
                    last_progress_update = @Last_progress_update,
                    max_point = @Max_point,
                    late_penalty = @Late_penalty,
                    allow_late = @Allow_late,
                    allow_resubmit = @Allow_resubmit,
                    pass_point = @Pass_point,
                    completed_at = @Completed_at
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
