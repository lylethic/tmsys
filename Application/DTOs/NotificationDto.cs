using System;

namespace server.Application.DTOs;

public class NotificationDto
{
    public string? Summary { get; set; }
    public string? Details { get; set; }
    public Guid User_id { get; set; }
    public int Main_category_type { get; set; }
    public int Sub_category_type { get; set; }
    public string? Group_code { get; set; }
    public string? Category_code { get; set; }
    public string? Sub_category_code { get; set; }
    public DateTime? Created_at { get; set; }
    public string? Reference_link { get; set; }
    public DateTime? Expired { get; set; }
    public DateTime? Sent_schedule { get; set; }
    public Guid? Status_id { get; set; }
    public string? Image { get; set; }
    public Guid[]? User_read { get; set; }
}
