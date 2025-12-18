#nullable disable
using server.Common.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Domain.Entities;

[Table("roles")]
public class Role : SystemLogEntity<Guid>
{
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public List<Permission> Permissions { get; set; } = [];
}
