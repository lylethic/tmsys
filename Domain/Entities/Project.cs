#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

[Table("projects")]
public partial class Project : SystemLogEntity<Guid>
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime Start_date { get; set; }

    public DateTime? End_date { get; set; }

    public Guid Manager_id { get; set; }

    public string? Status { get; set; }

    /// <summary>
    /// Type of the project (ex: IT, Marketing, BE, FE,...)
    /// </summary>
    public string? Project_type { get; set; }
}
