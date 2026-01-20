using server.Application.DTOs;

namespace server.Application.Common.Interfaces;

public interface IAttendanceService
{
    Task<CheckInResponse> CheckInAsync(Guid userId, CheckInRequest req, string? ip, string? userAgent, CancellationToken ct);
    Task<CheckInLocationResponse> ValidateLocationAsync(CheckInLocationRequest req, CancellationToken ct);
}
