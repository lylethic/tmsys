using System;
using server.Application.Request;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface IPermissionRepository
{
    Task<List<Permission>> GetUserPermissionsAsync(Guid userId);
    Task<List<Role>> GetUserRolesAsync(Guid userId);
    Task<bool> UserHasPermissionAsync(Guid userId, string permissionName);
    Task<bool> UserHasRoleAsync(Guid userId, string roleName);
    Task<User> GetUserWithRolesAndPermissionsAsync(Guid userId);
    Task<PaginatedResult<Permission>> GetPermissionsAsync(PaginationRequest pagination);
    Task<Permission> AddAsync(Permission permission);
    Task<Permission> UpdateAsync(Guid id, Permission permission);
    Task<string> DeleteAsync(Guid id);
}
