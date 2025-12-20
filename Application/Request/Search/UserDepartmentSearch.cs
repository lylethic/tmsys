using Microsoft.AspNetCore.Mvc;

namespace server.Application.Request.Search;

public class UserDepartmentSearch : DepartmentSearch
{
    [FromQuery(Name = "userId")]
    public Guid? UserId { get; set; }

    [FromQuery(Name = "departmentId")]
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Format: yyyy-MM-dd
    /// </summary>
    [FromQuery(Name = "startDate")]
    public string? Start_date { get; set; }

    /// <summary>
    /// Format: yyyy-MM-dd
    /// </summary>
    [FromQuery(Name = "endDate")]
    public string? End_date { get; set; }
}
