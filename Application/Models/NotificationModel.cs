using System.Text.Json.Serialization;
using server.Common.Domain.Entities;

namespace server.Application.Models;

public class NotificationModel : DomainModel
{
    public string Summary { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public Guid User_id { get; set; }
    public int Sub_category_type { get; set; }
    public string Group_code { get; set; } = string.Empty;
    public DateTime Created_at { get; set; }
    public string Reference_link { get; set; } = string.Empty;
    public int Main_category_type { get; set; }
    public DateTime? Expired { get; set; }
    public DateTime? Sent_schedule { get; set; }
    public Guid? Status_id { get; set; }
    public string Image { get; set; } = string.Empty;
    public Guid[] User_read { get; set; } = Array.Empty<Guid>();
    public string Status_name { get; set; } = string.Empty;
    public string Status_code { get; set; } = string.Empty;
    public string Status_color { get; set; } = string.Empty;
    public string Status_bgcolor { get; set; } = string.Empty;
    public bool Is_read { get; set; }
}

/// <summary>
/// Response model for notification (/me)
/// </summary>
public class NotificationRes
{
    public Guid Id { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public Guid User_id { get; set; }
    public int Sub_category_type { get; set; }
    public string Group_code { get; set; } = string.Empty;
    public DateTime Created_at { get; set; }
    public string Reference_link { get; set; } = string.Empty;
    public int Main_category_type { get; set; }
    public DateTime? Expired { get; set; }
    public DateTime? Sent_schedule { get; set; }
    public Guid? Status_id { get; set; }
    public string Image { get; set; } = string.Empty;
    public Guid[] User_read { get; set; } = Array.Empty<Guid>();
    public bool Is_read { get; set; }
    [JsonIgnore]
    public int? Total_count { get; set; }
}
