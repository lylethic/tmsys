#nullable disable
using Dapper.Contrib.Extensions;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

[Table("departments")]
public partial class Department : SystemLogEntity<Guid>
{

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>
    /// user_id of the parent department
    /// </summary>
    public Guid? Parent_id { get; set; } = null;
}
