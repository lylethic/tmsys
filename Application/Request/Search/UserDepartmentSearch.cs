using Microsoft.AspNetCore.Mvc;

namespace server.Application.Request.Search;

public class UserDepartmentSearch : DepartmentSearch
{
    [FromQuery(Name = "userId")]
    public Guid? UserId { get; set; }

    [FromQuery(Name = "departmentId")]
    public Guid? DepartmentId { get; set; }

    [FromQuery(Name = "startDate")]
    public string? Start_date { get; set; }

    [FromQuery(Name = "endDate")]
    public string? End_date { get; set; }
}
