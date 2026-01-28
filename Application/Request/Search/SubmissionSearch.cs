using System;
using Microsoft.AspNetCore.Mvc;
using server.Application.Request;

namespace server.Application.Request.Search;

public class SubmissionSearch : CursorPaginationRequest
{
    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; set; } = null;

    [FromQuery(Name = "taskId")]
    public Guid? Task_id { get; set; }

    [FromQuery(Name = "userId")]
    public Guid? User_id { get; set; }

    [FromQuery(Name = "status")]
    public string? Status { get; set; }

    [FromQuery(Name = "isLate")]
    public bool? Is_late { get; set; }

    [FromQuery(Name = "isPass")]
    public bool? Is_pass { get; set; }

    [FromQuery(Name = "submittedFrom")]
    public DateTime? Submitted_from { get; set; }

    [FromQuery(Name = "submittedTo")]
    public DateTime? Submitted_to { get; set; }

    [FromQuery(Name = "approvedStatusId")]
    public Guid? Approved_status_id { get; set; }
}
