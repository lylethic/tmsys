using System;
using server.Application.Common.Interfaces;
using server.Domain.Entities;

namespace server.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _repository;

    public PermissionService(IPermissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> UserHasPermissionAsync(Guid userId, string permission)
    {
        return await _repository.UserHasPermissionAsync(userId, permission);
    }

    public async Task<bool> UserHasRoleAsync(Guid userId, string role)
    {
        return await _repository.UserHasRoleAsync(userId, role);
    }

    public async Task<List<Permission>> GetUserPermissionsAsync(Guid userId)
    {
        return await _repository.GetUserPermissionsAsync(userId);
    }

    public async Task<List<Role>> GetUserRolesAsync(Guid userId)
    {
        return await _repository.GetUserRolesAsync(userId);
    }

    public async Task<List<Permission>> GetPermissionsByRoleAsync(string roleName)
    {
        return await _repository.GetPermissionsByRoleAsync(roleName);
    }
}
