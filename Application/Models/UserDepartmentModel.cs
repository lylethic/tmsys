using server.Domain.Entities;

namespace server.Application.Models;

public class UserDepartmentModel
{
    public UserDepartment UserDepartment { get; set; }
    public virtual User? User { get; set; }
    public virtual Department? Department { get; set; }
}
