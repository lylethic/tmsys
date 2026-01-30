using Microsoft.AspNetCore.Mvc;
using server.Application.Search;

namespace server.Application.Request.Search;

public class ProjectMemberSearch : CursorPaginationRequest
{
    [FromQuery(Name = "keyword")]
    public string? Keyword { get; set; }
    [FromQuery(Name = "projectId")]
    public Guid? ProjectId { get; set; }
    [FromQuery(Name = "memberId")]
    public Guid? MemberId { get; set; }
    [FromQuery(Name = "role")]
    public string? Role { get; set; }
    [FromQuery(Name = "leftAt")]
    public DateTime? LeftAt { get; set; }
    [FromQuery(Name = "sortOrder")]
    public string? SortOrderDirection { get; set; } // "asc" or "desc"
}
