using server.Domain.Entities;

namespace server.Application.Models;

public class UserDepartmentModel
{
    public UserDepartment UserDepartment { get; set; }
    public virtual ExtendUser? ExtendUser { get; set; }
    public virtual Department? ExtendDepartment { get; set; }
}
