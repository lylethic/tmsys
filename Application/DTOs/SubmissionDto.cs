using System;
using server.Common.Domain.Request.Create;
using server.Common.Domain.Request.Update;

namespace server.Application.DTOs;

public class SubmissionDto
{
    public Guid Task_id { get; set; }
    public Guid User_id { get; set; }
    public DateTime Submitted_at { get; set; }
    public bool Is_late { get; set; }
    public decimal? Raw_point { get; set; }
    public decimal? Penalty_point { get; set; }
    public decimal? Final_score { get; set; }
    public string Status { get; set; } = null!;
    public string? Note { get; set; }
    public int? Attempt_no { get; set; }
    public bool? Is_pass { get; set; }
    public Guid? Approved_status_id { get; set; }
}

public class SubmissionCreate : DomainCreate
{
    public Guid Task_id { get; set; }
    public Guid User_id { get; set; }
    public DateTime Submitted_at { get; set; }
    public bool Is_late { get; set; }
    public decimal? Raw_point { get; set; }
    public decimal? Penalty_point { get; set; }
    public decimal? Final_score { get; set; }
    public string Status { get; set; } = null!;
    public string? Note { get; set; }
    public int? Attempt_no { get; set; }
    public bool? Is_pass { get; set; }
    public Guid? Approved_status_id { get; set; }
}

public class SubmissionUpdate : DomainUpdate
{
    public Guid Task_id { get; set; }
    public Guid User_id { get; set; }
    public DateTime Submitted_at { get; set; }
    public bool Is_late { get; set; }
    public decimal? Raw_point { get; set; }
    public decimal? Penalty_point { get; set; }
    public decimal? Final_score { get; set; }
    public string Status { get; set; } = null!;
    public string? Note { get; set; }
    public int? Attempt_no { get; set; }
    public bool? Is_pass { get; set; }
    public Guid? Approved_status_id { get; set; }
}

/// <summary>
/// DTO for submitting a task - only requires task_id, user_id, raw_point (optional), and note
/// System will automatically calculate is_late, penalty, final_score, is_pass
/// </summary>
public class SubmitTaskRequest
{
    public Guid Task_id { get; set; }
    public Guid User_id { get; set; }
    public decimal? Raw_point { get; set; } // Optional - if not provided, will use task's max_point
    public string? Note { get; set; }
}

/// <summary>
/// DTO for leader to review and approve/reject a submission
/// Leader provides the final score and approval decision
/// </summary>
public class ReviewSubmissionRequest
{
    public decimal Raw_point { get; set; } // Score given by leader
    public bool Is_approved { get; set; } // Whether submission is approved
    public string? Review_note { get; set; } // Optional review comments from leader
}
