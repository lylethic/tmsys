using System;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface IPermissionService
{
    Task<bool> UserHasPermissionAsync(Guid userId, string permission);
    Task<bool> UserHasRoleAsync(Guid userId, string role);
    Task<List<Permission>> GetUserPermissionsAsync(Guid userId);
    Task<List<Role>> GetUserRolesAsync(Guid userId);
    Task<List<Permission>> GetPermissionsByRoleAsync(string roleName);
}