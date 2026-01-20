using System;

namespace server.Application.Models;

public sealed record CompanyGeofence(
    Guid Id,
    string Name,
    double CenterLat,
    double CenterLng,
    int RadiusM,
    bool IsActive
);

public sealed record AttendanceCheckin(
    Guid Id,
    Guid UserId,
    DateTimeOffset CheckinTime,
    DateOnly CheckinDate,
    double Lat,
    double Lng,
    int AccuracyM,
    int DistanceM,
    bool IsValid,
    string? Reason,
    string? DeviceId,
    string? Ip,
    string? UserAgent
);

