using server.Common.Domain.Entities;
using server.Common.Domain.Request.Create;
using server.Common.Domain.Request.Update;

namespace server.Application.DTOs;

public class ReportDto
{
    public Guid Project_id { get; set; }
    public DateTime Report_date { get; set; }
    public string Content { get; set; } = null!;
    public string Type { get; set; } = null!;
    public Guid? Generated_by { get; set; }
}

public class ReportCreate : DomainCreate
{
    public required Guid Project_id { get; set; }
    public required DateTime Report_date { get; set; }
    public required string Content { get; set; }
    public required string Type { get; set; }
}

public class ReportUpdate : DomainUpdate
{
    public Guid Project_id { get; set; }
    public DateTime Report_date { get; set; }
    public string? Content { get; set; }
    public string? Type { get; set; }
}


public class ReportModel : DomainModel
{
    public Guid Project_id { get; set; }
    public DateTime Report_date { get; set; }
    public string? Content { get; set; }
    public string? Type { get; set; }
    public Guid Generated_by { get; set; }
}
