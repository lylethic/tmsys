using System.Text.Json.Serialization;
using server.Domain.Entities;

namespace server.Application.Models;

/// <summary>
/// VIEW pg: v_notifications_details
/// </summary>
public class ExtendNotification
{
    public List<NotificationObject> Notifications { get; set; }

    // 'extend_user'
    [JsonPropertyName("extend_user")]
    public UserObject Extend_user { get; set; }

    // 'extend_status'
    [JsonPropertyName("extend_status")]
    public StatusObject Extend_status { get; set; }
}

public class ExtendNotificationRawData
{
    // Cột 'extend_user' (json)
    public string Extend_user { get; set; }

    // Cột 'extend_status' (json)
    public string Extend_status { get; set; }

    // Cột 'notifications' (json)
    // Cột này giờ là MỘT CHUỖI JSON chứa MỘT MẢNG
    public string Notifications { get; set; }
}

public class NotificationObject : Notification
{

}

public class UserObject
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Profilepic { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public bool Active { get; set; }
}
public class StatusObject
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Sort_order { get; set; }
    public string Bgcolor { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}