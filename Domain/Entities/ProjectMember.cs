#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

[Table("project_members")]
public partial class ProjectMember : SystemLogEntities<Guid>
{
    public Guid project_id { get; set; }

    public Guid member_id { get; set; }

    public string? role { get; set; }

    public DateTime? left_at { get; set; }
}
