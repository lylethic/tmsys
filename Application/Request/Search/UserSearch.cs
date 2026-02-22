using Microsoft.AspNetCore.Mvc;
using server.Application.Search;

namespace server.Application.Request.Search;

public class UserSearch : BaseSearch
{
    [FromQuery(Name = "active")]
    public bool? Active { get; set; }

    [FromQuery(Name = "deleted")]
    public bool? Deleted { get; set; }

    [FromQuery(Name = "role")]
    public string? Role { get; set; }
}
