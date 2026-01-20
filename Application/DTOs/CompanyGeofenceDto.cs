namespace server.Application.DTOs;

public sealed class CompanyGeofenceUpsert
{
    public string Name { get; set; } = string.Empty;
    public double CenterLat { get; set; }
    public double CenterLng { get; set; }
    public int RadiusM { get; set; }
    public bool? Active { get; set; } = true;
}
