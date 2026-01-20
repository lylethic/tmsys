using System;
using Microsoft.AspNetCore.Mvc;
using server.Application.Request;

namespace server.Application.Request.Search;

public class PopupSearch : CursorPaginationRequest
{
    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; set; } = null;

    /// <summary>
    /// keyword (content)
    /// </summary>
    [FromQuery(Name = "keyword")]
    public string? Keyword { get; set; }

    [FromQuery(Name = "type")]
    public short? Type { get; set; }

    [FromQuery(Name = "validityStart")]
    public DateTime? Validity_start { get; set; }

    [FromQuery(Name = "validityEnd")]
    public DateTime? Validity_end { get; set; }

    [FromQuery(Name = "displayFrom")]
    public DateTime? Display_from { get; set; }

    [FromQuery(Name = "displayTo")]
    public DateTime? Display_to { get; set; }

    [FromQuery(Name = "isActive")]
    public bool? Is_active { get; set; } = true;
}
