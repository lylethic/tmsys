#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

[Table("attendance_checkins")]
public partial class AttendanceCheckin : SystemLogEntity<Guid>
{
    public Guid User_id { get; set; }

    public DateTime Checkin_time { get; set; }

    public DateOnly Checkin_date { get; set; }

    public double Lat { get; set; }

    public double Lng { get; set; }

    public int Accuracy_m { get; set; }

    public int Distance_m { get; set; }

    public bool Is_valid { get; set; }

    public string? Reason { get; set; }

    public string? Device_id { get; set; }

    public IPAddress? Ip { get; set; }

    public string? User_agent { get; set; }
}
