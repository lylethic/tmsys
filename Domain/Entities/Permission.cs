#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

[Table("permission")]
public class Permission : SystemLogEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
