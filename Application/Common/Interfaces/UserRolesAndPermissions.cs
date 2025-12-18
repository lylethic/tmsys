using System;
using server.Domain.Entities;
namespace server.Application.Common.Interfaces;

public class UserRolesAndPermissions
{
    public List<Role> Roles { get; set; } = new();
    public List<Permission> Permissions { get; set; } = new();
}
