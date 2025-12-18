#nullable disable
using Dapper.Contrib.Extensions;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

[Table("user-deparments")]
public partial class UserDepartment : SystemLogEntity<Guid>
{
    public Guid user_id { get; set; }

    public Guid department_id { get; set; }

    public bool is_primary { get; set; }

    public DateOnly? start_date { get; set; }

    public DateOnly? end_date { get; set; }

    [Write(false)]
    public virtual Department Department { get; set; }

    [Write(false)]
    public virtual User User { get; set; }
}
