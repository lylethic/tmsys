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
    /// Dashboard tổng hợp: task + attendance (30 ngày) + work schedule.
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
    /// Tổng quan tasks: tổng số, phân theo status, quá hạn, hoàn thành đúng/trễ hạn, trung bình ngày hoàn thành.
    /// </summary>
    /// <param name="projectId">Lọc theo project (optional).</param>
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
    /// Thống kê task theo từng thành viên: tổng task, completed, in-progress, overdue, điểm TB, tỷ lệ on-time.
    /// </summary>
    /// <param name="projectId">Lọc theo project (optional).</param>
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
    /// Thống kê task theo từng project: tổng task, completed, in-progress, overdue, tỷ lệ hoàn thành.
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
    /// Tổng quan điểm danh: tổng check-in, hợp lệ/không hợp lệ, tỷ lệ, unique users, trend theo ngày.
    /// </summary>
    /// <param name="from">Ngày bắt đầu (yyyy-MM-dd).</param>
    /// <param name="to">Ngày kết thúc (yyyy-MM-dd).</param>
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
    /// Thống kê điểm danh theo từng thành viên: tổng check-in, hợp lệ, tỷ lệ, số ngày làm việc, tỷ lệ đi làm.
    /// </summary>
    /// <param name="from">Ngày bắt đầu (yyyy-MM-dd).</param>
    /// <param name="to">Ngày kết thúc (yyyy-MM-dd).</param>
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
    /// Tổng quan lịch làm việc: phân bổ ngày trong tuần, tổng intern, tổng mentor.
    /// </summary>
    /// <param name="weekStart">Tuần bắt đầu (optional).</param>
    /// <param name="weekEnd">Tuần kết thúc (optional).</param>
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
    /// Thống kê mentor load: mỗi mentor quản lý bao nhiêu intern.
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
