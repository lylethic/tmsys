using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Medo;
using Microsoft.AspNetCore.SignalR;
using server.Application.Common.Respository;
using server.Application.DTOs;
using server.Application.Models;
using server.Application.Request;
using server.Common.Exceptions;
using server.Common.Interfaces;
using server.Common.Providers;
using server.Domain.Entities;
using server.Hubs;

namespace server.Services;

public class NotificationService : SimpleCrudRepository<Notification, Guid>
{
    private readonly IAssistantService _assistantService;
    private readonly ILogManager _logger;
    private readonly IHubContext<NotificationHub, INotificationHub> _hubContext;
    private readonly INotificationCategoryProvider _categoryProvider;

    public NotificationService(
        IDbConnection connection,
        IAssistantService assistantService,
        ILogManager logger,
        IHubContext<NotificationHub, INotificationHub> hubContext,
        INotificationCategoryProvider categoryProvider) : base(connection)
    {
        _logger = logger;
        _assistantService = assistantService;
        _hubContext = hubContext;
        _categoryProvider = categoryProvider;
    }

    public async Task<PaginatedResult<NotificationModel>> GetAllAsync(NotificationSearchRequest? request)
    {
        request ??= new NotificationSearchRequest();
        var keyword = string.IsNullOrWhiteSpace(request.Keyword) ? null : request.Keyword.Trim();
        var groupCodeFilter = string.IsNullOrWhiteSpace(request.Category_code)
            ? (string.IsNullOrWhiteSpace(request.Group_code) ? null : request.Group_code.Trim())
            : request.Category_code.Trim();
        var currentUser = Guid.Parse(_assistantService.UserId);
        var filters = """
            WHERE (@UserId::UUID IS NULL OR n.user_id = @UserId::UUID)
                AND (@StatusId::UUID IS NULL OR n.status_id = @StatusId::UUID)
                AND (@MainCategoryType::INTEGER IS NULL OR n.main_category_type = @MainCategoryType::INTEGER)
                AND (@SubCategoryType::INTEGER IS NULL OR n.sub_category_type = @SubCategoryType::INTEGER)
                AND (@GroupCode::TEXT IS NULL OR n.group_code = @GroupCode::TEXT)
                AND (
                    @Keyword::TEXT IS NULL
                    OR n.summary ILIKE '%' || @Keyword::TEXT || '%'
                    OR n.details ILIKE '%' || @Keyword::TEXT || '%'
                )
                AND (@CreatedFrom::TIMESTAMP IS NULL OR n.created_at >= @CreatedFrom::TIMESTAMP)
                AND (@CreatedTo::TIMESTAMP IS NULL OR n.created_at <= @CreatedTo::TIMESTAMP)
                AND (
                    @OnlyUnread IS NULL
                    OR @OnlyUnread = FALSE
                    OR (
                        @OnlyUnread = TRUE
                        AND @CurrentUserId IS NOT NULL
                        AND NOT (
                            @CurrentUserId::UUID = ANY(
                                COALESCE(n.user_read, ARRAY[]::UUID[])
                            )
                        )
                    )
                )
        """;
        var query = $"""
            SELECT
                n.*,
                s.name AS status_name,
                s.code AS status_code,
                s.color AS status_color,
                s.bgcolor AS status_bgcolor
            FROM notifications AS n
            LEFT JOIN approved_status AS s ON s.id = n.status_id
            {filters}
            ORDER BY n.created_at DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;
            SELECT COUNT(*) FROM notifications AS n
            {filters};
        """;

        var parameters = new
        {
            Offset = (request.PageIndex - 1) * request.PageSize,
            PageSize = request.PageSize,
            UserId = request.User_id,
            StatusId = request.Status_id,
            MainCategoryType = request.Main_category_type,
            SubCategoryType = request.Sub_category_type,
            GroupCode = groupCodeFilter,
            Keyword = keyword,
            CreatedFrom = request.CreatedFrom,
            CreatedTo = request.CreatedTo,
            OnlyUnread = request.OnlyUnread,
            CurrentUserId = currentUser.ToString()
        };

        try
        {
            using var multi = await _connection.QueryMultipleAsync(query, parameters);
            var data = multi.Read<NotificationModel>().ToList();
            var total = multi.ReadSingle<int>();
            if (currentUser != Guid.Empty)
            {
                foreach (var item in data)
                {
                    item.Is_read = item.User_read?.Contains(currentUser) == true;
                }
            }

            return new PaginatedResult<NotificationModel>
            {
                Data = data,
                TotalCount = total,
                Page = request.PageIndex,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.Error($"Error fetching notifications: {ex.Message}");
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<bool> BulkDeleteAsync(List<Guid> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            throw new BadRequestException("Please provide a list of IDs to delete.");
        }

        try
        {
            var bulkDelete = """
                DELETE FROM notifications
                WHERE id = ANY(@Ids);
            """;
            var affectedRows = await _connection.ExecuteAsync(bulkDelete, new { Ids = ids });
            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error deleting notifications: {ex.Message}");
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<Notification> GetNotificationByIdAsync(Guid id)
    {
        var sql = """
            SELECT * FROM notifications
            WHERE id = @Id
            LIMIT 1;
        """;
        var entity = await _connection.QuerySingleOrDefaultAsync<Notification>(sql, new { Id = id });
        if (entity is null)
        {
            throw new BadRequestException("Notification not found.");
        }

        return entity;
    }

    public async Task<Notification> CreateNotificationAsync(NotificationDto dto)
    {
        var entity = new Notification
        {
            id = Uuid7.NewUuid7().ToGuid(),
            summary = string.Empty,
            details = string.Empty,
            user_id = dto.User_id
        };
        ApplyDtoToEntity(entity, dto, true);
        return await CreateAsync(entity);
    }

    public async Task<Notification> UpdateNotificationAsync(Guid id, NotificationDto dto)
    {
        var existing = await GetNotificationByIdAsync(id);
        ApplyDtoToEntity(existing, dto, false);
        return await UpdateAsync(existing);
    }

    public async Task<Notification> MarkAsReadAsync(Guid id)
    {
        var entity = await GetNotificationByIdAsync(id);
        var reader = Guid.Parse(_assistantService.UserId);
        if (reader != Guid.Empty)
        {
            var readers = entity.user_read?.ToList() ?? new List<Guid>();
            if (!readers.Contains(reader))
            {
                readers.Add(reader);
                entity.user_read = readers.ToArray();
            }
        }

        return await UpdateAsync(entity);
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId)
    {
        const string sql = """
            UPDATE notifications
            SET user_read = array_append(user_read, @UserId) -- array_append: add a new element to the end of an existing array
            WHERE user_id = @UserId
                AND NOT (@UserId = ANY(user_read));
        """;

        var affected = await _connection.ExecuteAsync(sql, new { UserId = userId });

        return affected;
    }


    public async Task SoftDeleteNotificationAsync(Guid id)
    {
        const string sql = "DELETE FROM notifications WHERE id = @Id;";
        var affectedRows = await _connection.ExecuteAsync(sql, new { Id = id });
        if (affectedRows == 0)
        {
            throw new BadRequestException("Notification not found.");
        }
    }

    public async Task CheckAndUpdateTaskNotificationsAsync()
    {
        try
        {
            _logger.Info("[TaskNotification] Starting task deadline check...");
            var startTime = DateTime.UtcNow;

            var tasksToNotify = await GetTasksRequiringNotificationAsync();
            if (!tasksToNotify.Any())
            {
                _logger.Info("[TaskNotification] No tasks require notification at this time.");
                return;
            }

            _logger.Info($"[TaskNotification] Found {tasksToNotify.Count()} tasks requiring notification. Processing...");
            var plannedNotifications = tasksToNotify.Select(task =>
            {
                var overdue = task.Days_Overdue_Or_Remaining > 0;
                var subCategoryCode = overdue ? "TASK_OVERDUE" : "TASK_DEADLINE";
                var subCategory = _categoryProvider.GetSubCategoryByCode(subCategoryCode);
                var reference = !string.IsNullOrWhiteSpace(subCategory?.Reference)
                ? subCategory.Reference
                : $"tasks/{task.Task_id}";

                return new
                {
                    Task = task,
                    Overdue = overdue,
                    SubCategory = subCategory,
                    Reference = reference
                };
            }).ToList();

            var existingLinks = await GetExistingUnreadNotificationsForTasks(plannedNotifications.Select(p => p.Reference));
            var notificationsToSend = new List<Notification>();

            foreach (var planned in plannedNotifications)
            {
                if (existingLinks.Contains(planned.Reference))
                {
                    continue;
                }

                string message;
                var overdue = planned.Overdue;
                if (overdue)
                {
                    message = $"Task `{planned.Task.Task_name}` đã quá hạn cập nhật {planned.Task.Days_Overdue_Or_Remaining} ngày (Đến hạn: {planned.Task.Next_Update_Due_Date:dd/MM}). Vui lòng cập nhật ngay!";
                }
                else
                {
                    message = $"Task `{planned.Task.Task_name}` đến hạn cập nhật hôm nay ({planned.Task.Next_Update_Due_Date:dd/MM}).";
                }

                var subCategory = planned.SubCategory;
                var mainTypeValue = subCategory?.ParentCategory?.TryGetTypeValue() ?? 300;
                var subTypeValue = subCategory?.TryGetTypeValue() ?? (overdue ? 302 : 301);
                var groupCode = subCategory?.ParentCategory?.GroupCode;
                if (string.IsNullOrWhiteSpace(groupCode))
                {
                    groupCode = subCategory?.ParentCategory?.Code ?? "TASK";
                }
                var catalogSummary = subCategory?.Name ?? (overdue ? "Task quá hạn cập nhật" : "Task đến hạn cập nhật");
                var catalogReference = planned.Reference;

                var newNotification = new Notification
                {
                    id = Uuid7.NewUuid7().ToGuid(),
                    summary = catalogSummary,
                    details = message,
                    user_id = planned.Task.User_Id_To_Notify,
                    sub_category_type = subTypeValue,
                    main_category_type = mainTypeValue,
                    group_code = groupCode,
                    created_at = DateTime.UtcNow,
                    reference_link = catalogReference,
                    sent_schedule = DateTime.UtcNow,
                    expired = DateTime.UtcNow.AddDays(7),
                    image = string.Empty,
                    user_read = Array.Empty<Guid>()
                };

                await CreateAsync(newNotification);
                notificationsToSend.Add(newNotification);
            }

            if (!notificationsToSend.Any())
            {
                _logger.Info("All pending notifications already exist.");
                return;
            }

            foreach (var notification in notificationsToSend)
            {
                try
                {
                    await _hubContext.Clients.Group(notification.user_id.ToString())
                        .ReceiveMessage("System", notification.details);
                }
                catch (Exception ex)
                {
                    _logger.Error($"[TaskNotification] Failed to send SignalR notification to user {notification.user_id}: {ex.Message}");
                }
            }

            var duration = (DateTime.UtcNow - startTime).TotalSeconds;
            _logger.Info($"[TaskNotification] Successfully created and sent {notificationsToSend.Count} notifications in {duration:F2}s");
        }
        catch (Exception ex)
        {
            _logger.Error($"[TaskNotification] Job execution failed: {ex.Message}", ex);
            // Don't throw to prevent job from failing
        }
    }

    private async Task<IEnumerable<TaskNotificationDto>> GetTasksRequiringNotificationAsync()
    {
        var sql = """
            SELECT
                t.id AS Task_Id,
                t.name AS Task_Name,
                t.assigned_to AS User_Id_To_Notify,
                (COALESCE(t.last_progress_update, t.created) + (t.update_frequency_days * INTERVAL '1 day'))::TIMESTAMP AS Next_Update_Due_Date,
                (CURRENT_DATE - (COALESCE(t.last_progress_update, t.created) + (t.update_frequency_days * INTERVAL '1 day'))::DATE) AS Days_Overdue_Or_Remaining
            FROM
                tasks t
            WHERE
                t.active = TRUE
                AND t.deleted = FALSE
                AND t.status NOT IN ('Done')
                AND (
                    (COALESCE(t.last_progress_update, t.created) + (t.update_frequency_days * INTERVAL '1 day')) <= (CURRENT_DATE + INTERVAL '1 day')
                )
            ORDER BY Next_Update_Due_Date;
        """;

        return await _connection.QueryAsync<TaskNotificationDto>(sql);
    }

    private async Task<HashSet<string>> GetExistingUnreadNotificationsForTasks(IEnumerable<string> references)
    {
        var refs = references.Where(r => !string.IsNullOrWhiteSpace(r)).ToArray();
        if (!refs.Any())
        {
            return new HashSet<string>();
        }

        var sql = """
            SELECT reference_link
            FROM notifications
            WHERE reference_link = ANY(@Refs)
                AND (expired IS NULL OR expired > CURRENT_TIMESTAMP)
        """;

        var results = await _connection.QueryAsync<string>(sql, new { Refs = refs });
        return results.ToHashSet();
    }

    private void ApplyDtoToEntity(Notification entity, NotificationDto dto, bool isNew)
    {
        if (dto is null)
            throw new BadRequestException("Notification payload is required.");

        if (dto.User_id == Guid.Empty)
            throw new BadRequestException("Recipient user ID is required.");

        entity.summary = string.IsNullOrWhiteSpace(dto.Summary) ? string.Empty : dto.Summary.Trim();
        entity.details = string.IsNullOrWhiteSpace(dto.Details) ? string.Empty : dto.Details.Trim();
        entity.user_id = dto.User_id;
        if (dto.Main_category_type > 0)
        {
            entity.main_category_type = dto.Main_category_type;
        }

        if (dto.Sub_category_type > 0)
        {
            entity.sub_category_type = dto.Sub_category_type;
        }

        if (!string.IsNullOrWhiteSpace(dto.Group_code))
        {
            entity.group_code = dto.Group_code.Trim();
        }

        ApplyCatalogCodes(entity, dto);
        entity.reference_link = string.IsNullOrWhiteSpace(dto.Reference_link) ? string.Empty : dto.Reference_link!.Trim();
        entity.expired = dto.Expired;
        entity.sent_schedule = dto.Sent_schedule ?? entity.sent_schedule ?? DateTime.UtcNow;
        entity.status_id = dto.Status_id;
        entity.image = dto.Image ?? string.Empty;

        if (dto.User_read is not null)
        {
            entity.user_read = dto.User_read;
        }

        if (isNew)
        {
            entity.id = entity.id == Guid.Empty ? Uuid7.NewUuid7().ToGuid() : entity.id;
            entity.created_at = dto.Created_at ?? DateTime.UtcNow;
        }
    }

    private void ApplyCatalogCodes(Notification entity, NotificationDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Category_code))
        {
            var category = _categoryProvider.GetCategoryByCode(dto.Category_code);
            if (category is not null)
            {
                if (category.TryGetTypeValue() is int mainType && mainType > 0)
                {
                    entity.main_category_type = mainType;
                }

                var groupCode = string.IsNullOrWhiteSpace(category.GroupCode)
                    ? category.Code
                    : category.GroupCode;

                if (!string.IsNullOrWhiteSpace(groupCode))
                {
                    entity.group_code = groupCode;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.Sub_category_code))
        {
            var subCategory = _categoryProvider.GetSubCategoryByCode(dto.Sub_category_code);
            if (subCategory is not null)
            {
                if (subCategory.TryGetTypeValue() is int subType && subType > 0)
                {
                    entity.sub_category_type = subType;
                }

                if (subCategory.ParentCategory?.TryGetTypeValue() is int parentType && parentType > 0)
                {
                    entity.main_category_type = parentType;
                }

                var groupCode = subCategory.ParentCategory?.GroupCode;
                if (string.IsNullOrWhiteSpace(groupCode))
                {
                    groupCode = subCategory.ParentCategory?.Code;
                }

                if (!string.IsNullOrWhiteSpace(groupCode))
                {
                    entity.group_code = groupCode;
                }
            }
        }
    }
}
