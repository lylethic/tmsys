using server.Domain.Entities;

namespace server.Application.Models;

public class ProjectMemberModel : ProjectMember
{
    public object? extend_project { get; set; }
    public object? extend_user { get; set; }
}
