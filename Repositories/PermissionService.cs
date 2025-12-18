using System;
using Microsoft.Extensions.Caching.Memory;
using server.Application.Common.Interfaces;
using server.Domain.Entities;

namespace server.Repositories;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public PermissionService(IPermissionRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<bool> UserHasPermissionAsync(Guid userId, string permission)
    {
        var cacheKey = $"user_permission_{userId}_{permission}";

        if (_cache.TryGetValue(cacheKey, out bool cachedResult))
        {
            return cachedResult;
        }

        var hasPermission = await _repository.UserHasPermissionAsync(userId, permission);
        _cache.Set(cacheKey, hasPermission, _cacheExpiration);

        return hasPermission;
    }

    public async Task<bool> UserHasRoleAsync(Guid userId, string role)
    {
        var cacheKey = $"user_role_{userId}_{role}";

        if (_cache.TryGetValue(cacheKey, out bool cachedResult))
        {
            return cachedResult;
        }

        var hasRole = await _repository.UserHasRoleAsync(userId, role);
        _cache.Set(cacheKey, hasRole, _cacheExpiration);

        return hasRole;
    }

    public async Task<List<Permission>> GetUserPermissionsAsync(Guid userId)
    {
        var cacheKey = $"user_permissions_{userId}";

        if (_cache.TryGetValue(cacheKey, out List<Permission> cachedPermissions))
        {
            return cachedPermissions;
        }

        var permissions = await _repository.GetUserPermissionsAsync(userId);
        _cache.Set(cacheKey, permissions, _cacheExpiration);

        return permissions;
    }

    public async Task<List<Role>> GetUserRolesAsync(Guid userId)
    {
        var cacheKey = $"user_roles_{userId}";

        if (_cache.TryGetValue(cacheKey, out List<Role> cachedRoles))
        {
            return cachedRoles;
        }

        var roles = await _repository.GetUserRolesAsync(userId);
        _cache.Set(cacheKey, roles, _cacheExpiration);

        return roles;
    }
}
