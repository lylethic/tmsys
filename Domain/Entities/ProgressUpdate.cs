#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

[Table("progress_updates")]
public partial class ProgressUpdate : SystemLogEntity<Guid>
{
    public Guid Task_id { get; set; }

    public Guid User_id { get; set; }

    public DateTime? Update_date { get; set; }

    public int Progress_percentage { get; set; }

    public string Notes { get; set; }
}
