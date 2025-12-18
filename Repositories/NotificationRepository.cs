using System;
using System.Data;
using System.Text.Json;
using Dapper;
using server.Application.Common.Respository;
using server.Application.Models;
using server.Application.Request;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Repositories;

public class NotificationRepository : SimpleCrudRepository<Notification, Guid>
{
    private readonly ILogManager _logManager;
    public NotificationRepository(IDbConnection connection, ILogManager logManager) : base(connection)
    {
        this._connection = connection;
        this._logManager = logManager;
    }

    /// <summary>
    /// Get notifications by user id and pagination
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Raw json data in db</returns>
    public async Task<PaginatedResult<ExtendNotificationRawData>> GetNotificationsAsync(PaginationRequest request)
    {
        var offset = (request.PageIndex - 1) * request.PageSize;
        const string sql = """
            SELECT COUNT(*) FROM v_notifications_grouped;

            SELECT *
            FROM v_notifications_grouped
            LIMIT @PageSize
            OFFSET @Offset;
        """;
        using var multi = await _connection.QueryMultipleAsync(
            sql,
            new { PageSize = request.PageSize, Offset = offset }
        );

        var result = new PaginatedResult<ExtendNotificationRawData>();

        // Total
        result.TotalCount = await multi.ReadSingleAsync<int>();

        // Data
        result.Data = [.. await multi.ReadAsync<ExtendNotificationRawData>()];

        return result;
    }

    /// <summary>
    /// Get user's notification list based on user ID extract from token. No filters.
    /// </summary>
    /// <param name="userID"></param>
    /// <param name="request"></param>
    /// <returns>List of notifications And total count</returns>
    public async Task<PaginatedResult<NotificationRes>> GetMyNotificationsAsync(Guid userID, PaginationRequest request)
    {
        try
        {
            var sql = """
                SELECT
                    id, 
                    summary, 
                    details, 
                    user_id, 
                    sub_category_type, 
                    group_code,
                    created_at, 
                    reference_link, 
                    main_category_type, 
                    expired, 
                    sent_schedule,
                    status_id, 
                    image, 
                    user_read,
                    (@UserId = ANY(user_read)) AS is_read, -- puts false first then true
                    COUNT(*) OVER() AS total_count
                FROM notifications
                WHERE user_id = @UserId
                ORDER BY is_read ASC, created_at DESC
                LIMIT @Limit 
                OFFSET @Offset;
            """;
            var parameters = new
            {
                UserId = userID,
                Limit = request.PageSize,
                Offset = (request.PageIndex - 1) * request.PageSize
            };
            var items = (await _connection.QueryAsync<NotificationRes>(sql, parameters)).ToList();
            int total = items.FirstOrDefault()?.Total_count ?? 0;

            return new PaginatedResult<NotificationRes>
            {
                Data = items,
                TotalCount = total,
                Page = request.PageIndex,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logManager.Error($"Error when getting my notifications for user with ID: {userID}", ex);
            throw;
        }
    }
}
