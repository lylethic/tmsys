using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using System.Data;

namespace server.Repositories;

public sealed class AttendanceOptions
{
    public string TimeZoneId { get; set; } = "Asia/Ho_Chi_Minh";
    public int MaxAccuracyMeters { get; set; } = 80;
}

public class AttendanceCheckinRepository : IAttendanceService
{
    private readonly AttendanceOptions _opt;
    private readonly TimeZoneInfo _tz;
    private readonly IDbConnection _connection;

    public AttendanceCheckinRepository(IDbConnection connection, IOptions<AttendanceOptions> opt)
    {
        _connection = connection;
        _opt = opt.Value;
        _tz = TimeZoneInfo.FindSystemTimeZoneById(_opt.TimeZoneId);
    }

    public async Task<CheckInResponse> CheckInAsync(Guid userId, CheckInRequest req, string? ip, string? userAgent, CancellationToken ct)
    {
        // 1) Validate request
        if (req.Lat is < -90 or > 90) throw new ArgumentException("Invalid latitude.");
        if (req.Lng is < -180 or > 180) throw new ArgumentException("Invalid longitude.");
        if (req.AccuracyM <= 0) throw new ArgumentException("Invalid accuracy.");

        // 2) Time (VN)
        var nowUtc = DateTimeOffset.UtcNow;
        var nowLocal = TimeZoneInfo.ConvertTime(nowUtc, _tz);
        var checkinDate = DateOnly.FromDateTime(nowLocal.DateTime);

        if (_connection.State != ConnectionState.Open)
            _connection.Open();

        // 3) Get active geofence
        var fence = await _connection.QuerySingleOrDefaultAsync<Domain.Entities.CompanyGeofence>(
            new CommandDefinition(@"
                select id, name, center_lat, center_lng, radius_m
                from company_geofences
                where active = true and deleted = false
                order by created desc
                limit 1;",
            cancellationToken: ct));

        if (fence is null)
        {
            return new CheckInResponse
            {
                IsValid = false,
                DistanceM = 0,
                FenceRadiusM = 0,
                CheckedInAt = nowLocal,
                Message = "Company geofence is not configured."
            };
        }

        // 4) Reject too poor accuracy
        if (req.AccuracyM > _opt.MaxAccuracyMeters)
        {
            // Return immediately (you can still log if needed)
            return new CheckInResponse
            {
                IsValid = false,
                DistanceM = 0,
                FenceRadiusM = fence.Radius_m,
                CheckedInAt = nowLocal,
                Message = $"GPS accuracy is too low ({req.AccuracyM}m). Please enable High accuracy and try again."
            };
        }

        // 5) Compute distance to center
        var distanceM = (int)Math.Round(HaversineMeters(req.Lat, req.Lng, fence.Center_lat, fence.Center_lng));
        var isValid = distanceM <= fence.Radius_m;
        var reason = isValid ? null : "out_of_fence";

        // 6) Insert (unique constraint prevents multiple check-ins per day)
        var checkinId = Guid.NewGuid();

        const string sqlInsert = @"
            insert into attendance_checkins
                (id, user_id, checkin_time, checkin_date, lat, lng, accuracy_m, distance_m, is_valid, reason, device_id, ip, user_agent)
            values
                (@Id, @UserId, @CheckinTime, @CheckinDate, @Lat, @Lng, @AccuracyM, @DistanceM, @IsValid, @Reason, @DeviceId, @Ip::inet, @UserAgent);";

        var entity = new Application.Models.AttendanceCheckin(
            Id: checkinId,
            UserId: userId,
            CheckinTime: nowUtc,
            CheckinDate: checkinDate,
            Lat: req.Lat,
            Lng: req.Lng,
            AccuracyM: req.AccuracyM,
            DistanceM: distanceM,
            IsValid: isValid,
            Reason: reason,
            DeviceId: req.DeviceId,
            Ip: ip,
            UserAgent: userAgent
        );

        try
        {
            await _connection.ExecuteAsync(new CommandDefinition(
                sqlInsert,
                new
                {
                    entity.Id,
                    entity.UserId,
                    entity.CheckinTime,
                    CheckinDate = entity.CheckinDate.ToDateTime(TimeOnly.MinValue), // Dapper maps DateOnly -> DateTime
                    entity.Lat,
                    entity.Lng,
                    entity.AccuracyM,
                    entity.DistanceM,
                    entity.IsValid,
                    entity.Reason,
                    entity.DeviceId,
                    Ip = entity.Ip,
                    UserAgent = entity.UserAgent
                },
                cancellationToken: ct
            ));
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") // unique_violation
        {
            return new CheckInResponse
            {
                IsValid = false,
                DistanceM = distanceM,
                FenceRadiusM = fence.Radius_m,
                CheckedInAt = nowLocal,
                Message = "You have already checked in today."
            };
        }

        return new CheckInResponse
        {
            IsValid = isValid,
            DistanceM = distanceM,
            FenceRadiusM = fence.Radius_m,
            CheckedInAt = nowLocal,
            Message = isValid ? "Check-in successful." : "You are outside the company area."
        };
    }

    public async Task<CheckInLocationResponse> ValidateLocationAsync(CheckInLocationRequest req, CancellationToken ct)
    {
        if (req.Lat is < -90 or > 90) throw new ArgumentException("Invalid latitude.");
        if (req.Lng is < -180 or > 180) throw new ArgumentException("Invalid longitude.");
        if (req.AccuracyM is <= 0) throw new ArgumentException("Invalid accuracy.");

        if (_connection.State != ConnectionState.Open)
            _connection.Open();

        var fence = await _connection.QuerySingleOrDefaultAsync<Domain.Entities.CompanyGeofence>(
            new CommandDefinition(@"
                select id, name, center_lat, center_lng, radius_m
                from company_geofences
                where active = true and deleted = false
                order by created desc
                limit 1;",
            cancellationToken: ct));

        if (fence is null)
        {
            return new CheckInLocationResponse
            {
                IsWithinFence = false,
                DistanceM = 0,
                FenceRadiusM = 0,
                Message = "Company geofence is not configured."
            };
        }

        if (req.AccuracyM.HasValue && req.AccuracyM > _opt.MaxAccuracyMeters)
        {
            return new CheckInLocationResponse
            {
                IsWithinFence = false,
                DistanceM = 0,
                FenceRadiusM = fence.Radius_m,
                Message = $"GPS accuracy is too low ({req.AccuracyM}m). Please enable High accuracy and try again."
            };
        }

        var distanceM = (int)Math.Round(HaversineMeters(req.Lat, req.Lng, fence.Center_lat, fence.Center_lng));
        var isWithinFence = distanceM <= fence.Radius_m;

        return new CheckInLocationResponse
        {
            IsWithinFence = isWithinFence,
            DistanceM = distanceM,
            FenceRadiusM = fence.Radius_m,
            Message = isWithinFence ? "Location is within the company area." : "Location is outside the company area."
        };
    }

    // Haversine distance (meters)
    private static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // Earth radius meters
        static double ToRad(double deg) => deg * Math.PI / 180.0;

        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

}
