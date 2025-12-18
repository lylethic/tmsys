using System;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using server.Application.Request;

namespace server.Application.Search;

public class BaseSearch : PaginationRequest
{
    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; set; } = null;
}

public class RequestBaseSearch : PaginationRequest
{
    public bool? Active { get; set; } = true;
    public bool? Deleted { get; set; } = false;
}
