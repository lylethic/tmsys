using System;
using Microsoft.AspNetCore.Mvc;
using server.Application.Search;

namespace server.Application.Request.Search;

public class ReportSearch : BaseSearch
{
    [FromQuery(Name = "reportDate")]
    public DateTime? P_report_date { get; set; }


    [FromQuery(Name = "projectId")]
    public Guid? Project_id { get; set; }


    [FromQuery(Name = "type")]
    public string? P_type { get; set; }
}
