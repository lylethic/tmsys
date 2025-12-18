using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using server.Application.Common.Interfaces;
using server.Common.Settings;

namespace server.Application.Common.Respository;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;

    public PermissionHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            context.Fail();
            return;
        }

        var hasPermission = await _permissionService.UserHasPermissionAsync(userId, requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}