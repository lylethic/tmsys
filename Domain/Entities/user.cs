#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

[Table("users")]
public class User : SystemLogEntity<Guid>
{
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; }

    public string Email { get; set; }
    public string City { get; set; } = string.Empty;

    public string Password { get; set; }

    public string? ProfilePic { get; set; } = string.Empty;

    public virtual Guid Role_id { get; set; }
    public DateTime Last_login_time { get; set; } = DateTime.UtcNow;
    public string Token { get; set; } = string.Empty;
    public bool? Is_send_email { get; set; }
    public string Profilepic_data { get; set; } = string.Empty;
    public virtual List<Permission> Permissions { get; set; } = [];
}