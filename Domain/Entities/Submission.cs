#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

[Table("submissions")]
public partial class Submission : SystemLogEntity<Guid>
{
    public Guid Task_id { get; set; }

    public Guid User_id { get; set; }

    public DateTime Submitted_at { get; set; }

    public bool Is_late { get; set; }

    public decimal? Raw_point { get; set; }

    public decimal? Penalty_point { get; set; }

    public decimal? Final_score { get; set; }

    public string Status { get; set; } = null!;

    public string? Note { get; set; }

    /// <summary>
    /// Lần nộp thứ mấy
    /// </summary>
    public int? Attempt_no { get; set; }

    public bool? Is_pass { get; set; }

    public Guid? Approved_status_id { get; set; }
}
