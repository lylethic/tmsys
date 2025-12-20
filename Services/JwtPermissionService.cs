using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Medo;
using Microsoft.Extensions.Caching.Memory;
using server.Application.Common.Interfaces;
using server.Domain.Entities;

namespace server.Services;

public class JwtPermissionService : IPermissionService
{
    private readonly IPermissionRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public JwtPermissionService(
        IPermissionRepository repository,
        IMemoryCache cache,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> UserHasPermissionAsync(Guid userId, string permission)
    {
        // First check JWT claims for better performance
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userIdFromClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdFromClaim, out var claimUserId) && claimUserId == userId)
            {
                var permissionClaims = user.FindAll("permission").Select(c => c.Value);
                if (permissionClaims.Contains(permission))
                {
                    return true;
                }
            }
        }

        // Fallback to database check with caching
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
        // First check JWT claims
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userIdFromClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdFromClaim, out var claimUserId) && claimUserId == userId)
            {
                var roleClaims = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
                if (roleClaims.Contains(role))
                {
                    return true;
                }
            }
        }

        // Fallback to database check with caching
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
        // First try to get from JWT claims
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userIdFromClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdFromClaim, out var claimUserId) && claimUserId == userId)
            {
                var permissionClaims = user.FindAll("permission").Select(c => c.Value).ToList();
                if (permissionClaims.Any())
                {
                    return permissionClaims.Select(p => new Permission { Id = Uuid7.NewUuid7().ToGuid(), Name = p }).ToList();
                }
            }
        }

        // Fallback to database
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
        // First try to get from JWT claims
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userIdFromClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdFromClaim, out var claimUserId) && claimUserId == userId)
            {
                var roleClaims = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                if (roleClaims.Any())
                {
                    return roleClaims.Select(r => new Role { Id = Uuid7.NewUuid7().ToGuid(), Name = r }).ToList();
                }
            }
        }

        // Fallback to database
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
