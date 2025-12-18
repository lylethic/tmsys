#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

[Table("project_types")]
public class ProjectType : SystemLogEntity<Guid>
{
    public string Name { get; set; }
    public string Description { get; set; }
}
