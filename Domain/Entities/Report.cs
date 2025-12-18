#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

[Table("reports")]
public partial class Report : SystemLogEntity<Guid>
{
    public Guid Project_id { get; set; }

    public DateTime Report_date { get; set; }

    public string Content { get; set; } = null!;

    public string Type { get; set; } = null!;

    public Guid? Generated_by { get; set; }
}
