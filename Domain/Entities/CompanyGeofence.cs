#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

[Table("company_geofences")]
public partial class CompanyGeofence : SystemLogEntity<Guid>
{
    public string Name { get; set; } = null!;

    public double Center_lat { get; set; }

    public double Center_lng { get; set; }

    public int Radius_m { get; set; }
}
