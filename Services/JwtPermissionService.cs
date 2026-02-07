using System.Security.Claims;
using Medo;
using server.Application.Common.Interfaces;
using server.Domain.Entities;

namespace server.Services;

public class JwtPermissionService : IPermissionService
{
    private readonly IPermissionRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtPermissionService(
        IPermissionRepository repository,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
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

        // Query database directly
        return await _repository.UserHasPermissionAsync(userId, permission);
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

        // Query database directly
        return await _repository.UserHasRoleAsync(userId, role);
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

        // Query database directly
        return await _repository.GetUserPermissionsAsync(userId);
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

        // Query database directly
        return await _repository.GetUserRolesAsync(userId);
    }

    public async Task<List<Permission>> GetPermissionsByRoleAsync(string roleName)
    {
        return await _repository.GetPermissionsByRoleAsync(roleName);
    }
}
