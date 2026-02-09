using System.Data;
using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.Models;
using server.Application.Request;
using server.Application.Request.Search;
using server.Common.Exceptions;
using server.Domain.Entities;
using server.Services;

namespace server.Repositories;

public class SubmissionRepository : SimpleCrudRepository<Submission, Guid>, ISubmissionRepository
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAssistantService _assistantService;

    public SubmissionRepository(IDbConnection connection,
    IUserRepository userRepository,
    IAssistantService assistantService,
    ITaskRepository taskRepository) : base(connection)
    {
        _connection = connection;
        _userRepository = userRepository;
        _assistantService = assistantService;
        _taskRepository = taskRepository;
    }

    public async Task<Submission> AddAsync(Submission entity)
    {
        if (entity == null)
            throw new BadRequestException("Please provide submission details.");

        var existingTask = await _taskRepository.GetByIdAsync(entity.Task_id);
        if (existingTask == null)
            throw new NotFoundException($"Task with ID '{entity.Task_id}' does not exist.");

        var existingUser = await _userRepository.GetByIdAsync(entity.User_id);
        if (existingUser == null)
            throw new NotFoundException($"User with ID '{entity.User_id}' does not exist.");

        entity.Id = Uuid7.NewUuid7().ToGuid();
        entity.Created_by = Guid.Parse(_assistantService.UserId);

        var sql = """
            INSERT INTO submissions (
                id, task_id, user_id, submitted_at, is_late, 
                raw_point, penalty_point, final_score, status, note,
                attempt_no, is_pass, approved_status_id,
                created, updated, created_by, updated_by, 
                deleted, active
            ) 
            VALUES (
                @Id, @Task_id, @User_id, @Submitted_at, @Is_late, 
                @Raw_point, @Penalty_point, @Final_score, @Status, @Note,
                @Attempt_no, @Is_pass, @Approved_status_id,
                @Created, @Updated, @Created_by, @Updated_by, 
                @Deleted, @Active
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
                throw new BadRequestException("Submission created, but failed to retrieve it.");
            }
            throw new BadRequestException("Failed to insert submission into the database.");
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
            var existingSubmission = await GetByIdAsync(id)
                ?? throw new NotFoundException("Submission not found");

            var sql = @"
                UPDATE submissions 
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
            throw new BadRequestException("Failed to delete submission.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<CursorPaginatedResult<SubmissionModel>> GetSubmissionPageAsync(SubmissionSearch request)
    {
        try
        {
            var where = new List<string>();
            var param = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                where.Add("(note ILIKE '%' || @SearchTerm || '%' OR status ILIKE '%' || @SearchTerm || '%')");
                param.Add("SearchTerm", request.SearchTerm);
            }

            if (request.Task_id.HasValue)
            {
                where.Add("task_id = @Task_id");
                param.Add("Task_id", request.Task_id.Value);
            }

            if (request.User_id.HasValue)
            {
                where.Add("user_id = @User_id");
                param.Add("User_id", request.User_id.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                where.Add("status = @Status");
                param.Add("Status", request.Status);
            }

            if (request.Is_late.HasValue)
            {
                where.Add("is_late = @Is_late");
                param.Add("Is_late", request.Is_late.Value);
            }

            if (request.Is_pass.HasValue)
            {
                where.Add("is_pass = @Is_pass");
                param.Add("Is_pass", request.Is_pass.Value);
            }

            if (request.Submitted_from.HasValue)
            {
                where.Add("submitted_at >= @Submitted_from");
                param.Add("Submitted_from", request.Submitted_from.Value);
            }

            if (request.Submitted_to.HasValue)
            {
                where.Add("submitted_at <= @Submitted_to");
                param.Add("Submitted_to", request.Submitted_to.Value);
            }

            if (request.Approved_status_id.HasValue)
            {
                where.Add("approved_status_id = @Approved_status_id");
                param.Add("Approved_status_id", request.Approved_status_id.Value);
            }

            var orderDirection = request.Ascending ? "ASC" : "DESC";

            var page = await GetListCursorBasedAsync<Submission>(
                request: request,
                extraWhere: string.Join(" AND ", where),
                extraParams: param,
                orderByColumn: "id",
                orderDirection: orderDirection,
                idColumn: "id"
            );

            var result = new CursorPaginatedResult<SubmissionModel>
            {
                NextCursor = page.NextCursor,
                NextCursorSortOrder = page.NextCursorSortOrder,
                HasNextPage = page.HasNextPage,
                Total = page.Total
            };

            if (page.Data.Count == 0)
                return result;

            var taskCache = new Dictionary<Guid, Tasks>();
            var userCache = new Dictionary<Guid, User>();

            foreach (var submission in page.Data)
            {
                if (!taskCache.TryGetValue(submission.Task_id, out var task))
                {
                    task = await _taskRepository.GetByIdAsync(submission.Task_id);
                    taskCache[submission.Task_id] = task;
                }

                if (!userCache.TryGetValue(submission.User_id, out var user))
                {
                    user = await _userRepository.GetByIdAsync(submission.User_id);
                    userCache[submission.User_id] = user;
                }

                result.Data.Add(MapToSubmissionModel(submission, task, user));
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public override async Task<Submission> GetByIdAsync(Guid id)
    {
        var sql = """
            SELECT * FROM submissions 
            WHERE id = @Id AND deleted = false
        """;

        var result = await _connection.QuerySingleOrDefaultAsync<Submission>(sql, new { Id = id })
            ?? throw new NotFoundException("Submission not found");
        return result;
    }

    public async Task<SubmissionModel> GetSubmissionAsync(Guid id)
    {
        var sql = """
            SELECT * FROM submissions 
            WHERE id = @Id AND deleted = false
        """;

        var submissionResult = await _connection.QuerySingleOrDefaultAsync<Submission>(sql, new { Id = id })
            ?? throw new NotFoundException("Submission not found");

        var extendTask = await _taskRepository.GetByIdAsync(submissionResult.Task_id);
        var extendUser = await _userRepository.GetByIdAsync(submissionResult.User_id);

        var result = MapToSubmissionModel(submissionResult, extendTask, extendUser);
        return result;
    }

    public async Task<bool> UpdateItemAsync(Guid id, Submission entity)
    {
        try
        {
            var existingSubmission = await GetByIdAsync(id)
                ?? throw new NotFoundException("Submission not found");

            entity.Id = id;

            var existingTask = await _taskRepository.GetByIdAsync(entity.Task_id);
            if (existingTask == null)
                throw new NotFoundException($"Task with ID '{entity.Task_id}' does not exist.");

            var existingUser = await _userRepository.GetByIdAsync(entity.User_id);
            if (existingUser == null)
                throw new NotFoundException($"User with ID '{entity.User_id}' does not exist.");

            entity.Updated_by = Guid.Parse(_assistantService.UserId);

            var sql = """
                UPDATE submissions
                SET task_id = @Task_id, 
                    user_id = @User_id, 
                    submitted_at = @Submitted_at, 
                    is_late = @Is_late, 
                    raw_point = @Raw_point, 
                    penalty_point = @Penalty_point, 
                    final_score = @Final_score, 
                    status = @Status, 
                    note = @Note,
                    attempt_no = @Attempt_no,
                    is_pass = @Is_pass,
                    approved_status_id = @Approved_status_id,
                    updated = @Updated, 
                    updated_by = @Updated_by
                WHERE id = @Id AND deleted = false
            """;

            var result = await _connection.ExecuteAsync(sql, entity);
            if (result > 0)
                return true;
            throw new BadRequestException("Failed to update submission.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    /// <summary>
    /// Submit a task with automatic scoring logic
    /// </summary>
    /// <param name="taskId">ID of the task to submit</param>
    /// <param name="userId">ID of the user submitting</param>
    /// <param name="rawPoint">Raw score (optional, defaults to task's max_point if not provided)</param>
    /// <param name="note">Submission notes</param>
    /// <returns>Created submission with calculated scores</returns>
    public async Task<Submission> SubmitTaskAsync(Guid taskId, Guid userId, decimal? rawPoint, string? note)
    {
        // Get task information
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new NotFoundException($"Task with ID '{taskId}' does not exist.");

        // Verify user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException($"User with ID '{userId}' does not exist.");

        // Check number of previous submissions
        var countSql = """
            SELECT COUNT(*) FROM submissions 
            WHERE task_id = @TaskId AND user_id = @UserId AND deleted = false
        """;
        var attemptCount = await _connection.QuerySingleAsync<int>(countSql, new { TaskId = taskId, UserId = userId });
        var currentAttempt = attemptCount + 1;

        // Check if resubmission is allowed
        if (attemptCount > 0 && task.Allow_resubmit == false)
            throw new BadRequestException("This task does not allow resubmission.");

        // Calculate is_late
        var submittedAt = DateTime.UtcNow;
        var isLate = task.Due_date.HasValue && submittedAt > task.Due_date.Value;

        // Check if late submission is allowed
        if (isLate && task.Allow_late == false)
            throw new BadRequestException("This task does not allow late submission.");

        // Calculate scores
        // If raw_point not provided, use task's max_point as default
        var calculatedRawPoint = rawPoint ?? task.Max_point ?? 0;

        // Calculate penalty if submitted late
        decimal penaltyPoint = 0;
        if (isLate && task.Late_penalty.HasValue)
        {
            penaltyPoint = task.Late_penalty.Value;
        }

        // Calculate final score
        var finalScore = calculatedRawPoint - penaltyPoint;
        if (finalScore < 0) finalScore = 0;

        // Determine status - submissions require leader review
        var isPass = task.Pass_point.HasValue && finalScore >= task.Pass_point.Value;
        var submissionStatus = "REVIEW";

        var submission = new Submission
        {
            Id = Uuid7.NewUuid7().ToGuid(),
            Task_id = taskId,
            User_id = userId,
            Submitted_at = submittedAt,
            Is_late = isLate,
            Raw_point = calculatedRawPoint,
            Penalty_point = penaltyPoint,
            Final_score = finalScore,
            Status = submissionStatus,
            Note = note,
            Attempt_no = currentAttempt,
            Is_pass = null,
            Created_by = Guid.Parse(_assistantService.UserId),
            Active = true,
            Deleted = false
        };

        var sql = """
            INSERT INTO submissions (
                id, task_id, user_id, submitted_at, is_late, 
                raw_point, penalty_point, final_score, status, note,
                attempt_no, is_pass, approved_status_id,
                created, updated, created_by, updated_by, 
                deleted, active
            ) 
            VALUES (
                @Id, @Task_id, @User_id, @Submitted_at, @Is_late, 
                @Raw_point, @Penalty_point, @Final_score, @Status, @Note,
                @Attempt_no, @Is_pass, @Approved_status_id,
                @Created, @Updated, @Created_by, @Updated_by, 
                @Deleted, @Active
            )
        """;

        try
        {
            var queryResult = await _connection.ExecuteAsync(sql, submission);

            if (queryResult > 0)
            {
                // Note: Task status will be updated only after leader review and approval
                var result = await GetByIdAsync(submission.Id);
                if (result != null)
                    return result;
                throw new BadRequestException("Submission created, but failed to retrieve it.");
            }
            throw new BadRequestException("Failed to insert submission into the database.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    /// <summary>
    /// Leader reviews and approves/rejects a submission
    /// - Updates raw_point based on leader's assessment
    /// - Recalculates final_score with penalties
    /// - Determines is_pass based on pass_point threshold
    /// - Updates task status to Completed if approved and passed
    /// </summary>
    /// <param name="submissionId">ID of the submission to review</param>
    /// <param name="reviewerId">ID of the leader/reviewer</param>
    /// <param name="rawPoint">Score given by leader</param>
    /// <param name="isApproved">Whether the submission is approved</param>
    /// <param name="reviewNote">Review notes from leader</param>
    /// <returns>Updated submission with final scores and status</returns>
    public async Task<Submission> ReviewSubmissionAsync(Guid submissionId, Guid reviewerId, decimal rawPoint, bool isApproved, string? reviewNote)
    {
        // Get existing submission
        var submission = await GetByIdAsync(submissionId);
        if (submission == null)
            throw new NotFoundException($"Submission with ID '{submissionId}' does not exist.");

        // Check if submission is in reviewable state
        if (submission.Status != "Pending Review")
            throw new BadRequestException($"Submission cannot be reviewed. Current status: {submission.Status}");

        // Get task information for validation
        var task = await _taskRepository.GetByIdAsync(submission.Task_id);
        if (task == null)
            throw new NotFoundException($"Task with ID '{submission.Task_id}' does not exist.");

        // Verify reviewer exists
        var reviewer = await _userRepository.GetByIdAsync(reviewerId);
        if (reviewer == null)
            throw new NotFoundException($"Reviewer with ID '{reviewerId}' does not exist.");

        // Recalculate scores with leader's raw_point
        var updatedRawPoint = rawPoint;
        var penaltyPoint = submission.Penalty_point ?? 0; // Keep existing penalty

        // Calculate new final score
        var finalScore = updatedRawPoint - penaltyPoint;
        if (finalScore < 0) finalScore = 0;

        // Determine if passed based on pass_point threshold
        var isPass = isApproved && task.Pass_point.HasValue && finalScore >= task.Pass_point.Value;

        // Determine final status
        string finalStatus;
        if (!isApproved)
            finalStatus = "REJECTED";
        else if (isPass)
            finalStatus = "APPROVED";
        else
            finalStatus = "APPROVED";

        var updateSql = """
            UPDATE submissions
            SET raw_point = @RawPoint,
                final_score = @FinalScore,
                status = @Status,
                is_pass = @IsPass,
                note = CASE 
                    WHEN @ReviewNote IS NOT NULL THEN 
                        CASE 
                            WHEN note IS NULL OR note = '' THEN @ReviewNote
                            ELSE note || E'\n--- Leader Review ---\n' || @ReviewNote
                        END
                    ELSE note
                END,
                updated = @Updated,
                updated_by = @UpdatedBy
            WHERE id = @Id AND deleted = false
        """;

        var updateParams = new
        {
            Id = submissionId,
            RawPoint = updatedRawPoint,
            FinalScore = finalScore,
            Status = finalStatus,
            IsPass = isPass,
            ReviewNote = reviewNote,
            Updated = DateTime.UtcNow,
            UpdatedBy = reviewerId
        };

        try
        {
            var result = await _connection.ExecuteAsync(updateSql, updateParams);

            if (result > 0)
            {
                // Update task status if approved and passed
                if (isPass && task.Status != "COMPLETED")
                {
                    var updateTaskSql = """
                        UPDATE tasks 
                        SET status = 'COMPLETED', 
                            completed_at = @CompletedAt, 
                            updated = @Updated
                        WHERE id = @TaskId
                    """;
                    await _connection.ExecuteAsync(updateTaskSql, new
                    {
                        TaskId = task.Id,
                        CompletedAt = DateTime.UtcNow,
                        Updated = DateTime.UtcNow
                    });
                }

                var updatedSubmission = await GetByIdAsync(submissionId);
                if (updatedSubmission != null)
                    return updatedSubmission;
                throw new BadRequestException("Submission reviewed, but failed to retrieve updated data.");
            }
            throw new BadRequestException("Failed to update submission review.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    private static SubmissionModel MapToSubmissionModel(Submission submissionResult, Tasks? extendTask, User? extendUser)
    {
        return new SubmissionModel
        {
            Id = submissionResult.Id,
            Task_id = submissionResult.Task_id,
            User_id = submissionResult.User_id,
            Submitted_at = submissionResult.Submitted_at,
            Is_late = submissionResult.Is_late,
            Raw_point = submissionResult.Raw_point,
            Penalty_point = submissionResult.Penalty_point,
            Final_score = submissionResult.Final_score,
            Status = submissionResult.Status,
            Note = submissionResult.Note,
            Attempt_no = submissionResult.Attempt_no,
            Is_pass = submissionResult.Is_pass,
            Approved_status_id = submissionResult.Approved_status_id,
            Created = submissionResult.Created,
            Updated = submissionResult.Updated,
            Created_by = submissionResult.Created_by,
            Updated_by = submissionResult.Updated_by,
            Deleted = submissionResult.Deleted,
            Active = submissionResult.Active,
            Extend_task = extendTask != null ? new
            {
                name = extendTask.Name,
                description = extendTask.Description,
                status = extendTask.Status,
                due_date = extendTask.Due_date,
                priority = extendTask.Priority,
                update_frequency_days = extendTask.Update_frequency_days,
                last_progress_update = extendTask.Last_progress_update,
                max_point = extendTask.Max_point,
                late_penalty = extendTask.Late_penalty,
                allow_late = extendTask.Allow_late,
                allow_resubmit = extendTask.Allow_resubmit,
                pass_point = extendTask.Pass_point,
                completed_at = extendTask.Completed_at,
            } : null,
            Extend_user = extendUser != null ? new
            {
                name = extendUser.Name,
                profilepic = extendUser.ProfilePic
            } : null
        };
    }

}
