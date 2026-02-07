using System;
using System.Text.Json.Serialization;
using server.Common.Domain.Entities;
using server.Common.Domain.Request.Create;
using server.Common.Domain.Request.Update;
using server.Common.Interfaces;
using server.Domain.Entities;

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

public class ProjectModel : Project
{
    public object? extend_users { get; set; } = null;
    public object? extend_project_type { get; set; } = null;
}

public class ProjectRawDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Start_date { get; set; }
    public DateTime? End_date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Extend_users_json { get; set; }
    public string? Extend_project_type_json { get; set; }
}

public class ProjectExtendedModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Start_date { get; set; }
    public DateTime? End_date { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<ProjectMemberInfo>? Extend_users { get; set; } = null;
    public ProjectTypeInfo? Extend_project_type { get; set; } = null;
}

public class ProjectMemberInfo
{
    [JsonPropertyName("member_id")]
    public string Member_id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("profilepic")]
    public string? Profilepic { get; set; }
}

public class ProjectTypeInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

