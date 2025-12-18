using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace server.Common.Models;

public class NotificationCategoryConfig
{
    [JsonPropertyName("notification")]
    public NotificationCatalog Notification { get; set; } = new();
}

public class NotificationCatalog
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("categories")]
    public List<NotificationCategoryDefinition> Categories { get; set; } = new();
}

public class NotificationCategoryDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("group_code")]
    public string GroupCode { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public int? Priority { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("sub_categories")]
    public List<NotificationSubCategoryDefinition> SubCategories { get; set; } = new();

    /// <summary>
    /// Parses the <see cref="Type"/> value into an integer if possible.
    /// </summary>
    public int? TryGetTypeValue()
    {
        return int.TryParse(Type, out var value) ? value : null;
    }
}

public class NotificationSubCategoryDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("reference")]
    public string Reference { get; set; } = string.Empty;

    [JsonIgnore]
    public NotificationCategoryDefinition? ParentCategory { get; internal set; }

    public int? TryGetTypeValue()
    {
        return int.TryParse(Type, out var value) ? value : null;
    }
}
