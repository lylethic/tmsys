#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

[Table("user_roles")]
public class User_roles : SystemLogEntity<Guid>
{
    public Guid User_id { get; set; }

    public Guid Role_id { get; set; }
}
