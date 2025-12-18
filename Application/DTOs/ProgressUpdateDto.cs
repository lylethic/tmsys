using System;
using server.Common.Domain.Request.Create;
using server.Common.Domain.Request.Update;

namespace server.Application.DTOs;

public class ProgressUpdateDto
{
    public Guid Task_id { get; set; }
    public Guid User_id { get; set; }
    public DateTime Update_date { get; set; }
    /// <summary>
    /// phần trăm tiến độ
    /// </summary>
    public int Progress_percentage { get; set; }
    public string? Notes { get; set; }
}

public class ProgressUpdateCreate : DomainCreate
{
    public Guid Task_id { get; set; }
    public Guid User_id { get; set; }
    public DateTime Update_date { get; set; }
    /// <summary>
    /// phần trăm tiến độ
    /// </summary>
    public int Progress_percentage { get; set; }
    public string? Notes { get; set; }
}

public class ProgressUpdateUpdate : DomainUpdate
{
    public Guid Task_id { get; set; }
    public Guid User_id { get; set; }
    public DateTime Update_date { get; set; }
    /// <summary>
    /// phần trăm tiến độ
    /// </summary>
    public int Progress_percentage { get; set; }
    public string? Notes { get; set; }
}
