#nullable disable
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Domain.Entities;

[Table("role_permissions")]
public class Role_permissions
{
    public Guid Role_id { get; set; }
    public Guid Permission_id { get; set; }
}
