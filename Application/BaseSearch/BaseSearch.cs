using System;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using server.Application.Request;

namespace server.Application.Search;

public class BaseSearch : CursorPaginationRequest
{
    [FromQuery(Name = "keyword")]
    public string? Keyword { get; set; }
}
