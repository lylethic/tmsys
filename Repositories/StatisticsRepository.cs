using Dapper;
using server.Application.DTOs;
using server.Application.Request;
using System.Data;

namespace server.Repositories;

/// <summary>
/// Repository containing statistical queries for Tasks, Attendance, Work Schedule.
/// </summary>
public class StatisticsRepository
{
    private readonly IDbConnection _connection;

    public StatisticsRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    // ════════════════════════════════════════════
    // 1. TASK STATISTICS
    // ════════════════════════════════════════════

    /// <summary>
    /// Task overview: total, by status, overdue, completed on-time/late, average completion days.
    /// </summary>
    public async Task<TaskOverviewDto> GetTaskOverviewAsync(Guid? projectId = null)
    {
        if (_connection.State != ConnectionState.Open) _connection.Open();

        var projectFilter = projectId.HasValue ? "AND project_id = @ProjectId" : "";
        var param = new { ProjectId = projectId };

        // Status counts
        var statusSql = $"""
            SELECT status AS Status, COUNT(*) AS Count
            FROM tasks
            WHERE deleted = false AND active = true {projectFilter}
            GROUP BY status
            ORDER BY count DESC
        """;
        var statusCounts = (await _connection.QueryAsync<StatusCountDto>(statusSql, param)).ToList();

        // Overdue (not completed and past due date)
        var overdueSql = $"""
            SELECT COUNT(*) FROM tasks
            WHERE deleted = false AND active = true
              AND due_date < NOW() AT TIME ZONE 'UTC'
              AND (LOWER(status) != 'done' AND LOWER(status) != 'completed')
              {projectFilter}
        """;
        var overdue = await _connection.ExecuteScalarAsync<int>(overdueSql, param);

        // Completed on-time vs late
        var completionSql = $"""
            SELECT
                COUNT(*) FILTER (WHERE completed_at IS NOT NULL AND completed_at <= due_date) AS on_time,
                COUNT(*) FILTER (WHERE completed_at IS NOT NULL AND completed_at > due_date) AS late
            FROM tasks
            WHERE deleted = false AND active = true
              AND completed_at IS NOT NULL
              {projectFilter}
        """;
        var completion = await _connection.QuerySingleAsync<dynamic>(completionSql, param);

        // Average completion days
        var avgDaysSql = $"""
            SELECT COALESCE(AVG(EXTRACT(EPOCH FROM (completed_at - created)) / 86400), 0)
            FROM tasks
            WHERE deleted = false AND active = true
              AND completed_at IS NOT NULL
              {projectFilter}
        """;
        var avgDays = await _connection.ExecuteScalarAsync<double>(avgDaysSql, param);

        // Total
        var totalSql = $"""
            SELECT COUNT(*) FROM tasks
            WHERE deleted = false AND active = true {projectFilter}
        """;
        var total = await _connection.ExecuteScalarAsync<int>(totalSql, param);

        return new TaskOverviewDto
        {
            TotalTasks = total,
            ByStatus = statusCounts,
            OverdueTasks = overdue,
            CompletedOnTime = (int)(completion?.on_time ?? 0),
            CompletedLate = (int)(completion?.late ?? 0),
            AvgCompletionDays = Math.Round(avgDays, 1)
        };
    }

    /// <summary>
    /// Task statistics by member (can filter by project).
    /// </summary>
    public async Task<CursorPaginatedResult<MemberTaskStatsDto>> GetMemberTaskStatsAsync(
        Guid? projectId = null, Guid? cursor = null, int pageSize = 20, bool ascending = false)
    {
        if (_connection.State != ConnectionState.Open) _connection.Open();

        var projectFilter = projectId.HasValue ? "AND t.project_id = @ProjectId" : "";
        var orderDirection = ascending ? "ASC" : "DESC";
        var cursorCondition = cursor.HasValue
            ? (ascending ? "AND u.id > @Cursor" : "AND u.id < @Cursor")
            : "";

        var sql = $"""
            SELECT
                u.id         AS UserId,
                u.name       AS Name,
                u.email      AS Email,
                COUNT(t.id)  AS TotalTasks,
                COUNT(t.id) FILTER (WHERE LOWER(t.status) IN ('done','completed'))   AS Completed,
                COUNT(t.id) FILTER (WHERE LOWER(t.status) = 'in_progress')           AS InProgress,
                COUNT(t.id) FILTER (
                    WHERE t.due_date < NOW() AT TIME ZONE 'UTC'
                      AND LOWER(t.status) NOT IN ('done','completed')
                )                                                                     AS Overdue,
                ROUND(AVG(s.final_score), 2)                                          AS AvgScore,
                CASE
                    WHEN COUNT(t.id) FILTER (WHERE t.completed_at IS NOT NULL) = 0 THEN 0
                    ELSE ROUND(
                        COUNT(t.id) FILTER (WHERE t.completed_at IS NOT NULL AND t.completed_at <= t.due_date)::numeric
                        / NULLIF(COUNT(t.id) FILTER (WHERE t.completed_at IS NOT NULL), 0) * 100, 1
                    )
                END AS OnTimeRate
            FROM users u
            INNER JOIN tasks t ON t.assigned_to = u.id AND t.deleted = false AND t.active = true {projectFilter}
            LEFT JOIN LATERAL (
                SELECT final_score
                FROM submissions sub
                WHERE sub.task_id = t.id AND sub.user_id = u.id AND sub.deleted = false
                ORDER BY sub.attempt_no DESC NULLS LAST
                LIMIT 1
            ) s ON TRUE
            WHERE u.deleted = false AND u.active = true
              {cursorCondition}
            GROUP BY u.id, u.name, u.email
            ORDER BY u.id {orderDirection}
            LIMIT @PageSize
        """;

        var data = (await _connection.QueryAsync<MemberTaskStatsDto>(sql, new
        {
            ProjectId = projectId,
            Cursor = cursor,
            PageSize = pageSize + 1
        })).ToList();

        var hasNextPage = data.Count > pageSize;
        if (hasNextPage) data.RemoveAt(data.Count - 1);

        return new CursorPaginatedResult<MemberTaskStatsDto>
        {
            Data = data,
            NextCursor = hasNextPage && data.Count > 0 ? data[^1].UserId : null,
            HasNextPage = hasNextPage
        };
    }

    /// <summary>
    /// Task statistics by project.
    /// </summary>
    public async Task<List<ProjectTaskStatsDto>> GetProjectTaskStatsAsync()
    {
        if (_connection.State != ConnectionState.Open) _connection.Open();

        var sql = """
            SELECT
                p.id         AS ProjectId,
                p.name       AS ProjectName,
                COUNT(t.id)  AS TotalTasks,
                COUNT(t.id) FILTER (WHERE LOWER(t.status) IN ('done','completed'))   AS Completed,
                COUNT(t.id) FILTER (WHERE LOWER(t.status) = 'in_progress')           AS InProgress,
                COUNT(t.id) FILTER (
                    WHERE t.due_date < NOW() AT TIME ZONE 'UTC'
                      AND LOWER(t.status) NOT IN ('done','completed')
                ) AS Overdue,
                CASE
                    WHEN COUNT(t.id) = 0 THEN 0
                    ELSE ROUND(
                        COUNT(t.id) FILTER (WHERE LOWER(t.status) IN ('done','completed'))::numeric
                        / COUNT(t.id) * 100, 1
                    )
                END AS CompletionRate
            FROM projects p
            LEFT JOIN tasks t ON t.project_id = p.id AND t.deleted = false AND t.active = true
            WHERE p.deleted = false AND p.active = true
            GROUP BY p.id, p.name
            ORDER BY TotalTasks DESC
        """;

        return (await _connection.QueryAsync<ProjectTaskStatsDto>(sql)).ToList();
    }

    // ════════════════════════════════════════════
    // 2. ATTENDANCE STATISTICS
    // ════════════════════════════════════════════

    /// <summary>
    /// Attendance overview in time range, with daily trend.
    /// </summary>
    public async Task<AttendanceOverviewDto> GetAttendanceOverviewAsync(DateOnly? from, DateOnly? to)
    {
        if (_connection.State != ConnectionState.Open) _connection.Open();

        var dateFilter = BuildDateFilter(from, to);
        var param = new { From = from?.ToDateTime(TimeOnly.MinValue), To = to?.ToDateTime(TimeOnly.MinValue) };

        // Overview
        var overviewSql = $"""
            SELECT
                COUNT(*)                                          AS TotalCheckins,
                COUNT(*) FILTER (WHERE is_valid = true)           AS ValidCheckins,
                COUNT(*) FILTER (WHERE is_valid = false)          AS InvalidCheckins,
                CASE WHEN COUNT(*) = 0 THEN 0
                     ELSE ROUND(COUNT(*) FILTER (WHERE is_valid = true)::numeric / COUNT(*) * 100, 1)
                END                                               AS ValidRate,
                COUNT(DISTINCT user_id)                           AS UniqueUsers
            FROM attendance_checkins
            WHERE deleted = false {dateFilter}
        """;
        var overview = await _connection.QuerySingleAsync<AttendanceOverviewDto>(overviewSql, param);

        // Daily trend
        var trendSql = $"""
            SELECT
                TO_CHAR(checkin_date, 'YYYY-MM-DD')               AS Date,
                COUNT(*)                                          AS Total,
                COUNT(*) FILTER (WHERE is_valid = true)           AS Valid,
                COUNT(*) FILTER (WHERE is_valid = false)          AS Invalid
            FROM attendance_checkins
            WHERE deleted = false {dateFilter}
            GROUP BY checkin_date
            ORDER BY checkin_date
        """;
        overview.DailyTrend = (await _connection.QueryAsync<DailyCheckinDto>(trendSql, param)).ToList();


        return overview;
    }

    /// <summary>
    /// Attendance statistics by member.
    /// Includes attendance rate based on registered days in work_schedule.
    /// </summary>
    public async Task<CursorPaginatedResult<MemberAttendanceStatsDto>> GetMemberAttendanceStatsAsync(
        DateOnly? from, DateOnly? to, Guid? cursor = null, int pageSize = 20, bool ascending = false)
    {
        if (_connection.State != ConnectionState.Open) _connection.Open();

        var dateFilter = BuildDateFilter(from, to, "ac");
        var orderDirection = ascending ? "ASC" : "DESC";
        var cursorCondition = cursor.HasValue
            ? (ascending ? "AND u.id > @Cursor" : "AND u.id < @Cursor")
            : "";

        var sql = $"""
            WITH schedule_days AS (
                SELECT
                    ws.intern_id,
                    COUNT(DISTINCT d.day) AS working_days
                FROM work_schedule ws
                CROSS JOIN LATERAL (
                    SELECT unnest(ARRAY[
                        CASE WHEN ws.monday    IS NOT NULL AND ws.monday    != '' THEN 1 END,
                        CASE WHEN ws.tuesday   IS NOT NULL AND ws.tuesday   != '' THEN 2 END,
                        CASE WHEN ws.wednesday IS NOT NULL AND ws.wednesday != '' THEN 3 END,
                        CASE WHEN ws.thursday  IS NOT NULL AND ws.thursday  != '' THEN 4 END,
                        CASE WHEN ws.friday    IS NOT NULL AND ws.friday    != '' THEN 5 END
                    ]) AS day
                ) d
                WHERE ws.deleted = false AND ws.active = true
                  AND ws.intern_id IS NOT NULL
                  AND d.day IS NOT NULL
                GROUP BY ws.intern_id
            )
            SELECT
                u.id                                               AS UserId,
                u.name                                             AS Name,
                u.email                                            AS Email,
                COUNT(ac.id)                                       AS TotalCheckins,
                COUNT(ac.id) FILTER (WHERE ac.is_valid = true)     AS ValidCheckins,
                COUNT(ac.id) FILTER (WHERE ac.is_valid = false)    AS InvalidCheckins,
                CASE WHEN COUNT(ac.id) = 0 THEN 0
                     ELSE ROUND(COUNT(ac.id) FILTER (WHERE ac.is_valid = true)::numeric / COUNT(ac.id) * 100, 1)
                END                                                AS ValidRate,
                COALESCE(sd.working_days, 0)                       AS WorkingDays,
                CASE WHEN COALESCE(sd.working_days, 0) = 0 THEN 0
                     ELSE ROUND(
                        COUNT(DISTINCT ac.checkin_date) FILTER (WHERE ac.is_valid = true)::numeric
                        / sd.working_days * 100, 1
                     )
                END                                                AS AttendanceRate
            FROM users u
            LEFT JOIN attendance_checkins ac ON ac.user_id = u.id AND ac.deleted = false {dateFilter}
            LEFT JOIN schedule_days sd ON sd.intern_id = u.id
            WHERE u.deleted = false AND u.active = true
              AND (ac.id IS NOT NULL OR sd.intern_id IS NOT NULL)
              {cursorCondition}
            GROUP BY u.id, u.name, u.email, sd.working_days
            ORDER BY u.id {orderDirection}
            LIMIT @PageSize
        """;

        var data = (await _connection.QueryAsync<MemberAttendanceStatsDto>(sql, new
        {
            From = from?.ToDateTime(TimeOnly.MinValue),
            To = to?.ToDateTime(TimeOnly.MinValue),
            Cursor = cursor,
            PageSize = pageSize + 1
        })).ToList();

        var hasNextPage = data.Count > pageSize;
        if (hasNextPage) data.RemoveAt(data.Count - 1);

        return new CursorPaginatedResult<MemberAttendanceStatsDto>
        {
            Data = data,
            NextCursor = hasNextPage && data.Count > 0 ? data[^1].UserId : null,
            HasNextPage = hasNextPage
        };
    }

    // ════════════════════════════════════════════
    // 3. WORK SCHEDULE STATISTICS
    // ════════════════════════════════════════════

    /// <summary>
    /// Work schedule overview: day distribution, total interns/mentors.
    /// </summary>
    public async Task<WorkScheduleOverviewDto> GetWorkScheduleOverviewAsync(DateTimeOffset? weekStart = null, DateTimeOffset? weekEnd = null)
    {
        if (_connection.State != ConnectionState.Open) _connection.Open();

        // Filter for overlapping schedules: schedule overlaps if it starts before/on end date AND ends after/on start date
        var weekFilter = "";
        if (weekStart.HasValue && weekEnd.HasValue)
        {
            weekFilter = " AND ws.week_start <= @WeekEnd AND ws.week_end >= @WeekStart";
        }
        else if (weekStart.HasValue)
        {
            weekFilter = " AND ws.week_end >= @WeekStart";
        }
        else if (weekEnd.HasValue)
        {
            weekFilter = " AND ws.week_start <= @WeekEnd";
        }

        var param = new
        {
            WeekStart = weekStart?.ToUniversalTime(),
            WeekEnd = weekEnd?.ToUniversalTime()
        };

        var sql = $"""
            SELECT
                COUNT(*)                                                                AS TotalSchedules,
                COUNT(DISTINCT COALESCE(ws.intern_id::text, ws.intern_email))           AS TotalInterns,
                COUNT(DISTINCT ws.mentor_email)                                         AS TotalMentors,
                COUNT(*) FILTER (WHERE ws.monday    IS NOT NULL AND ws.monday    != '') AS Monday,
                COUNT(*) FILTER (WHERE ws.tuesday   IS NOT NULL AND ws.tuesday   != '') AS Tuesday,
                COUNT(*) FILTER (WHERE ws.wednesday IS NOT NULL AND ws.wednesday != '') AS Wednesday,
                COUNT(*) FILTER (WHERE ws.thursday  IS NOT NULL AND ws.thursday  != '') AS Thursday,
                COUNT(*) FILTER (WHERE ws.friday    IS NOT NULL AND ws.friday    != '') AS Friday
            FROM work_schedule ws
            WHERE ws.deleted = false AND ws.active = true
              {weekFilter}
        """;

        var row = await _connection.QuerySingleAsync<dynamic>(sql, param);

        return new WorkScheduleOverviewDto
        {
            TotalSchedules = (int)(row.totalschedules ?? 0),
            TotalInterns = (int)(row.totalinterns ?? 0),
            TotalMentors = (int)(row.totalmentors ?? 0),
            DayDistribution = new DayDistributionDto
            {
                Monday = (int)(row.monday ?? 0),
                Tuesday = (int)(row.tuesday ?? 0),
                Wednesday = (int)(row.wednesday ?? 0),
                Thursday = (int)(row.thursday ?? 0),
                Friday = (int)(row.friday ?? 0)
            }
        };
    }

    /// <summary>
    /// Mentor load statistics: how many interns each mentor manages.
    /// </summary>
    public async Task<List<MentorLoadDto>> GetMentorLoadAsync()
    {
        if (_connection.State != ConnectionState.Open) _connection.Open();

        var sql = """
            SELECT
                ws.mentor_email                                                    AS MentorEmail,
                COUNT(DISTINCT COALESCE(ws.intern_id::text, ws.intern_email))      AS InternCount
            FROM work_schedule ws
            WHERE ws.deleted = false AND ws.active = true
            GROUP BY ws.mentor_email
            ORDER BY InternCount DESC
        """;

        return (await _connection.QueryAsync<MentorLoadDto>(sql)).ToList();
    }

    // ════════════════════════════════════════════
    // 4. DASHBOARD (Comprehensive)
    // ════════════════════════════════════════════

    /// <summary>
    /// Comprehensive dashboard: task overview + attendance overview (last 30 days) + work schedule overview.
    /// </summary>
    public async Task<DashboardDto> GetDashboardAsync()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var to = DateOnly.FromDateTime(DateTime.UtcNow);

        var tasks = await GetTaskOverviewAsync();
        var attendance = await GetAttendanceOverviewAsync(from, to);
        var schedule = await GetWorkScheduleOverviewAsync();

        return new DashboardDto
        {
            Tasks = tasks,
            Attendance = attendance,
            WorkSchedule = schedule
        };
    }

    // ════════════════════════════════════════════
    // Helpers
    // ════════════════════════════════════════════

    private static string BuildDateFilter(DateOnly? from, DateOnly? to, string alias = "")
    {
        var prefix = string.IsNullOrEmpty(alias) ? "" : $"{alias}.";
        var filter = "";
        if (from.HasValue) filter += $" AND {prefix}checkin_date >= @From";
        if (to.HasValue) filter += $" AND {prefix}checkin_date <= @To";
        return filter;
    }
}
