#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

[Table("otp_codes")]
public class Otps : SystemLogEntity<Guid>
{
    public string Code { get; set; }
    public Guid User_id { get; set; }
    public bool Is_used { get; set; }
    public DateTime Created_at { get; set; }
    public DateTime Expire_at { get; set; }

}
