using Microsoft.AspNetCore.Mvc;
using server.Application.Search;

namespace server.Application.Request.Search;

public class UserSearch : BaseSearch
{
    [FromQuery(Name = "email")]
    public string? Email { get; set; }

    [FromQuery(Name = "name")]
    public string? Name { get; set; }

    [FromQuery(Name = "active")]
    public bool? Active { get; set; }

    [FromQuery(Name = "deleted")]
    public bool? Deleted { get; set; }
}
