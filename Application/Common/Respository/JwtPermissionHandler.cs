using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using server.Application.Common.Interfaces;
using server.Common.Settings;

namespace server.Application.Common.Respository;

public class JwtPermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;

    public JwtPermissionHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var user = context.User;
        if (!user.Identity.IsAuthenticated)
        {
            context.Fail();
            return;
        }

        // Get user's roles from JWT claims
        var roleNames = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        if (!roleNames.Any())
        {
            context.Fail();
            return;
        }

        // Check if any of the user's roles contains the required permission
        foreach (var roleName in roleNames)
        {
            var permissions = await _permissionService.GetPermissionsByRoleAsync(roleName);
            if (permissions.Any(p => p.Name == requirement.Permission))
            {
                context.Succeed(requirement);
                return;
            }
        }

        context.Fail();
    }
}
