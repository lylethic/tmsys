using System;
using server.Common.Domain.Request.Create;
using server.Common.Domain.Request.Update;

namespace server.Application.DTOs;

public class TaskDto
{
    public Guid Project_id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid Assigned_to { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? Due_date { get; set; }
    public int? Priority { get; set; }
    public int Update_frequency_days { get; set; }
    public DateTime Last_progress_update { get; set; }
}

public class TaskCreate : DomainCreate
{
    public Guid Project_id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid Assigned_to { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? Due_date { get; set; }
    public int? Priority { get; set; }
    public int Update_frequency_days { get; set; }
    public DateTime Last_progress_update { get; set; }
}

public class TaskUpdate : DomainUpdate
{
    public Guid Project_id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid Assigned_to { get; set; }
    public string? Status { get; set; }
    public DateTime? Due_date { get; set; }
    public int Priority { get; set; }
    public int Update_frequency_days { get; set; }
    public DateTime Last_progress_update { get; set; }
}

