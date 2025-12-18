using Microsoft.AspNetCore.Mvc;

namespace server.Application.Request;

public class DepartmentSearch : CursorPaginationRequest
{
    /// <summary>
    /// keyword (name, description)
    /// </summary>
    [FromQuery(Name = "keyword")]
    public string? Keyword { get; set; }
}
