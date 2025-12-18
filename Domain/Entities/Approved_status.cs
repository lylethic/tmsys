#nullable disable
using Dapper.Contrib.Extensions;
using server.Common.Domain.Entities;

namespace server.Domain.Entities;

[Table("approved_status")]
public class Approved_status : BaseEntity<Guid>
{
    public string name { get; set; } = string.Empty;
    public string code { get; set; } = string.Empty;
    public string color { get; set; } = string.Empty;
    public int? sort_order { get; set; }
    public string bgcolor { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
}
