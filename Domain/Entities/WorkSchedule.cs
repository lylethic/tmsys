using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;
using server.Domain.Entities;

namespace server.Domain.Entities;

[Table("work_schedule")]
public partial class Work_schedule : SystemLogEntity<Guid>
{
    public Guid? Intern_id { get; set; }

    public string? Intern_email { get; set; }

    public string? Mentor_email { get; set; }

    public string? Monday { get; set; }

    public string? Tuesday { get; set; }

    public string? Wednesday { get; set; }

    public string? Thursday { get; set; }

    public string? Friday { get; set; }

    public string? Full_name { get; set; }

    public virtual User? intern { get; set; }
}
