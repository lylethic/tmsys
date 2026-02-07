using System;
using System.Threading.Tasks;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using server.Application.DTOs;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;
using server.Repositories;

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/statistics")]
public class StatisticsController : BaseApiController
{
    private readonly ClientRequestLogRepository _requestLogRepo;
    private readonly StatisticsRepository _statsRepo;

    public StatisticsController(
        IServiceProvider provider,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger) : base(mapper, httpContextAccessor, logger)
    {
        _requestLogRepo = provider.GetRequiredService<ClientRequestLogRepository>();
        _statsRepo = provider.GetRequiredService<StatisticsRepository>();
    }

    // ════════════════════════════════════════════
    // DASHBOARD
    // ════════════════════════════════════════════

    /// <summary>
    /// Comprehensive dashboard: task + attendance (30 days) + work schedule.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var result = await _statsRepo.GetDashboardAsync();
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    // ════════════════════════════════════════════
    // TASK STATISTICS
    // ════════════════════════════════════════════

    /// <summary>
    /// Task overview: total, by status, overdue, completed on-time/late, average completion days.
    /// </summary>
    /// <param name="projectId">Filter by project (optional).</param>
    [HttpGet("tasks/overview")]
    public async Task<IActionResult> GetTaskOverview([FromQuery] Guid? projectId)
    {
        try
        {
            var result = await _statsRepo.GetTaskOverviewAsync(projectId);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Task statistics by member: total tasks, completed, in-progress, overdue, average score, on-time rate.
    /// </summary>
    /// <param name="projectId">Filter by project (optional).</param>
    /// <param name="cursor">Cursor for pagination (user ID from previous page).</param>
    /// <param name="pageSize">Number of items per page (default 20).</param>
    /// <param name="ascending">Sort order (default false - descending by ID).</param>
    [HttpGet("tasks/by-member")]
    public async Task<IActionResult> GetMemberTaskStats(
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? cursor = null,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool ascending = false)
    {
        try
        {
            var result = await _statsRepo.GetMemberTaskStatsAsync(projectId, cursor, pageSize, ascending);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Task statistics by project: total tasks, completed, in-progress, overdue, completion rate.
    /// </summary>
    [HttpGet("tasks/by-project")]
    public async Task<IActionResult> GetProjectTaskStats()
    {
        try
        {
            var result = await _statsRepo.GetProjectTaskStatsAsync();
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    // ════════════════════════════════════════════
    // ATTENDANCE STATISTICS
    // ════════════════════════════════════════════

    /// <summary>
    /// Attendance overview: total check-ins, valid/invalid, rate, unique users, daily trend.
    /// </summary>
    /// <param name="from">Start date (yyyy-MM-dd).</param>
    /// <param name="to">End date (yyyy-MM-dd).</param>
    [HttpGet("attendance/overview")]
    public async Task<IActionResult> GetAttendanceOverview([FromQuery] string? from, [FromQuery] string? to)
    {
        try
        {
            DateOnly? fromDate = null;
            DateOnly? toDate = null;

            if (!string.IsNullOrEmpty(from) && !DateOnly.TryParse(from, out var parsedFrom))
                return Error("Invalid 'from' date format. Use yyyy-MM-dd.");
            else if (!string.IsNullOrEmpty(from))
                fromDate = DateOnly.Parse(from);

            if (!string.IsNullOrEmpty(to) && !DateOnly.TryParse(to, out var parsedTo))
                return Error("Invalid 'to' date format. Use yyyy-MM-dd.");
            else if (!string.IsNullOrEmpty(to))
                toDate = DateOnly.Parse(to);

            var result = await _statsRepo.GetAttendanceOverviewAsync(fromDate, toDate);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Attendance statistics by member: total check-ins, valid, rate, working days, attendance rate.
    /// </summary>
    /// <param name="from">Start date (yyyy-MM-dd).</param>
    /// <param name="to">End date (yyyy-MM-dd).</param>
    /// <param name="cursor">Cursor for pagination (user ID from previous page).</param>
    /// <param name="pageSize">Number of items per page (default 20).</param>
    /// <param name="ascending">Sort order (default false - descending by ID).</param>
    [HttpGet("attendance/by-member")]
    public async Task<IActionResult> GetMemberAttendanceStats(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] Guid? cursor = null,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool ascending = false)
    {
        try
        {
            DateOnly? fromDate = null;
            DateOnly? toDate = null;

            if (!string.IsNullOrEmpty(from) && !DateOnly.TryParse(from, out var parsedFrom))
                return Error("Invalid 'from' date format. Use yyyy-MM-dd.");
            else if (!string.IsNullOrEmpty(from))
                fromDate = DateOnly.Parse(from);

            if (!string.IsNullOrEmpty(to) && !DateOnly.TryParse(to, out var parsedTo))
                return Error("Invalid 'to' date format. Use yyyy-MM-dd.");
            else if (!string.IsNullOrEmpty(to))
                toDate = DateOnly.Parse(to);

            var result = await _statsRepo.GetMemberAttendanceStatsAsync(fromDate, toDate, cursor, pageSize, ascending);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    // ════════════════════════════════════════════
    // WORK SCHEDULE STATISTICS
    // ════════════════════════════════════════════

    /// <summary>
    /// Work schedule overview: day distribution in week, total interns, total mentors.
    /// </summary>
    /// <param name="weekStart">Week start (optional).</param>
    /// <param name="weekEnd">Week end (optional).</param>
    [HttpGet("schedule/overview")]
    public async Task<IActionResult> GetWorkScheduleOverview(
        [FromQuery] DateTimeOffset? weekStart,
        [FromQuery] DateTimeOffset? weekEnd)
    {
        try
        {
            var result = await _statsRepo.GetWorkScheduleOverviewAsync(weekStart, weekEnd);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Mentor load statistics: how many interns each mentor manages.
    /// </summary>
    [HttpGet("schedule/mentor-load")]
    public async Task<IActionResult> GetMentorLoad()
    {
        try
        {
            var result = await _statsRepo.GetMentorLoadAsync();
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    // ════════════════════════════════════════════
    // CLIENT REQUEST LOG (existing)
    // ════════════════════════════════════════════

    // /// <summary>
    // /// Creates a new client request log.
    // /// </summary>
    // /// <param name="dto"></param>
    // /// <returns></returns>
    // [HttpPost("request-log")]
    // public async Task<IActionResult> Create([FromBody] Client_request_logCreate dto)
    // {
    //     var entity = _mapper.Map<Client_request_log>(dto);
    //     await _requestLogRepo.CreateAsync(entity);
    //     _logger.Info($"Created client request log with id {entity.ToJson()}");
    //     return Success("Created successfully");
    // }

    // [HttpGet("ip")]
    // public IActionResult GetClientIp()
    // {
    //     string ip = HttpContext.Connection.RemoteIpAddress?.ToString();

    //     if (string.IsNullOrEmpty(ip) && HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
    //     {
    //         ip = HttpContext.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0];
    //     }

    //     return Success(new { IpAddress = ip });
    // }

    // [HttpGet("most-access")]
    // public async Task<IActionResult> GetMostAccessedFeatures()
    // {
    //     var result = await _requestLogRepo.GetMostAccessedFeaturesAsync();
    //     return Success(result);
    // }
}
