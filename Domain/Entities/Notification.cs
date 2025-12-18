#nullable disable
using Dapper.Contrib.Extensions;

namespace server.Domain.Entities;

[Table("notifications")]
public partial class Notification
{
  [ExplicitKey] // Guid key - not auto-increment
  public Guid id { get; set; }

  public string summary { get; set; }

  public string details { get; set; }

  public Guid user_id { get; set; }

  public int sub_category_type { get; set; }

  public string group_code { get; set; } = string.Empty;

  public DateTime created_at { get; set; }

  public string reference_link { get; set; } = string.Empty;

  public int main_category_type { get; set; }

  public DateTime? expired { get; set; }

  public DateTime? sent_schedule { get; set; }

  public Guid? status_id { get; set; }

  public string image { get; set; } = string.Empty;

  public Guid[] user_read { get; set; } = Array.Empty<Guid>();

  [Write(false)]
  public virtual Approved_status? status_ { get; set; }
}
