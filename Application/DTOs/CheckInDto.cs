
namespace server.Application.DTOs;

public sealed class CheckInRequest
{
    public double Lat { get; set; }
    public double Lng { get; set; }
    public int AccuracyM { get; set; }
    public string? DeviceId { get; set; }
}

public sealed class CheckInResponse
{
    public bool IsValid { get; set; }
    public int DistanceM { get; set; }
    public int FenceRadiusM { get; set; }
    public DateTimeOffset CheckedInAt { get; set; }
    public string Message { get; set; } = "";
}

public sealed class CheckInLocationRequest
{
    public double Lat { get; set; }
    public double Lng { get; set; }
    public int? AccuracyM { get; set; }
}

public sealed class CheckInLocationResponse
{
    public bool IsWithinFence { get; set; }
    public int DistanceM { get; set; }
    public int FenceRadiusM { get; set; }
    public string Message { get; set; } = "";
}
