using System;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using server.Application.Search;

namespace server.Application.Request.Search;

public class ProjectSearch : BaseSearch
{
    [FromQuery(Name = "projectId")]
    public Guid? ProjectId { get; set; }
    [FromQuery(Name = "managerId")]
    public Guid? ManagerId { get; set; }
    [FromQuery(Name = "memberId")]
    public Guid? MemberId { get; set; }
    [FromQuery(Name = "startDate")]
    public DateTime? StartDate { get; set; }

    [FromQuery(Name = "endDate")]
    public DateTime? EndDate { get; set; }

    [FromQuery(Name = "status")]
    public string? Status { get; set; }
    [FromQuery(Name = "sort")]
    public string? SortOrderDirection { get; set; }
}
