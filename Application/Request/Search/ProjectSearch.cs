using System;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using server.Application.Search;

namespace server.Application.Request.Search;

public class ProjectSearch : BaseSearch
{
    [FromQuery(Name = "startDate")]
    public DateTime? P_start_date { get; set; }

    [FromQuery(Name = "endDate")]
    public DateTime? P_end_date { get; set; }
}
