using System;
using server.Common.Domain.Request.Create;
using server.Common.Domain.Request.Update;

namespace server.Application.DTOs;

public class ProjectMemberCreate : DomainCreate
{
    public Guid project_id { get; set; }

    public Guid member_id { get; set; }

    public string? role { get; set; }
}

public class ProjectMemberUpdate : DomainUpdate
{
    public Guid project_id { get; set; }

    public Guid member_id { get; set; }

    public string? role { get; set; }

    public DateTime? left_at { get; set; }
}
