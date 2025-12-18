using System;

namespace server.Application.Request;

public class NotificationSearchRequest : PaginationRequest
{
    public Guid? User_id { get; set; }
    public Guid? Status_id { get; set; }
    public int? Main_category_type { get; set; }
    public int? Sub_category_type { get; set; }
    public string? Group_code { get; set; }
    public string? Category_code { get; set; }
    public string? Sub_category_code { get; set; }
    public string? Keyword { get; set; }
    public bool? OnlyUnread { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}
