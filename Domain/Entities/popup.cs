using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

[Table("popups")]
public partial class Popup : SystemLogEntity<Guid>
{
    public string Content { get; set; } = null!;

    public DateTime Validity_start { get; set; }

    public DateTime Validity_end { get; set; }

    public short Type { get; set; }

    public DateTime? Display_from { get; set; }

    public DateTime? Display_to { get; set; }
}
