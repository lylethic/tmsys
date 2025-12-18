using Microsoft.AspNetCore.Mvc;

namespace server.Application.Request.Search;

public class ApprovedSearch : CursorPaginationRequest
{
    /// <summary>
    /// keyword (name, description)
    /// </summary>
    [FromQuery(Name = "keyword")]
    public string? Keyword { get; set; }
    /// <summary>
    /// type: task, noti,...
    /// </summary>
    [FromQuery(Name = "type")]
    public string? Type { get; set; }

    // sorting direction on sort_order: "asc" or "desc"
    [FromQuery(Name = "sort_order")]
    public string? SortOrderDirection { get; set; } // "asc" or "desc"
}
