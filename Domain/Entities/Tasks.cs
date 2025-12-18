#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

[Table("tasks")]
public partial class Tasks : SystemLogEntity<Guid>
{
    public Guid Project_id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public Guid Assigned_to { get; set; }

    public string Status { get; set; }

    public DateTime? Due_date { get; set; }

    public int? Priority { get; set; }
    public int Update_frequency_days { get; set; } // Số ngày tối đa giữa các lần cập nhật tiến độ (mặc định là 7 ngày)
    public DateTime Last_progress_update { get; set; } // Lần cập nhật tiến độ cuối cùng
}
