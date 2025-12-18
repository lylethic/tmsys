using System;
using server.Common.Domain.Entities;
using server.Common.Domain.Request.Create;
using server.Common.Domain.Request.Update;
using server.Common.Interfaces;

namespace server.Application.DTOs;

public class ProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Start_date { get; set; }
    public DateTime? End_date { get; set; }
    public Guid Manager_id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Project_type { get; set; } = string.Empty;
}

public class ProjectCreate : DomainCreate
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Start_date { get; set; }
    public DateTime? End_date { get; set; }
    public Guid Manager_id { get; set; }
    public string Project_type { get; set; } = string.Empty;
}

public class ProjectUpdate : DomainUpdate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Start_date { get; set; }
    public DateTime? End_date { get; set; }
    public Guid Manager_id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Project_type { get; set; } = string.Empty;
}

public class ProjectModel : DomainModel, IHasTotalCount
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Start_date { get; set; }
    public DateTime? End_date { get; set; }
    public Guid Manager_id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Project_type { get; set; } = string.Empty;
}

