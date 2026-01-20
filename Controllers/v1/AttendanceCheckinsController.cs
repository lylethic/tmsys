using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Services;

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/attendance/checkins")]
public class AttendanceCheckinsController : BaseApiController
{
    private readonly IAttendanceService _attendanceService;
    private readonly IAssistantService _assistantService;
    public AttendanceCheckinsController(
        IAttendanceService attendanceService,
        IAssistantService assistantService,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger) : base(mapper, httpContextAccessor, logger)
    {
        _attendanceService = attendanceService;
        _assistantService = assistantService;
    }

    [HttpGet("validate-location")]
    public async Task<IActionResult> ValidateLocation([FromQuery] CheckInLocationRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _attendanceService.ValidateLocationAsync(request, ct);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request, CancellationToken ct)
    {
        try
        {
            var userId = Guid.Parse(_assistantService.UserId);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

            var result = await _attendanceService.CheckInAsync(userId, request, ip, userAgent, ct);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }
}
