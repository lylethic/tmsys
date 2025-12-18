using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using server.Application.Common.Interfaces;

namespace server.Common.Settings;

public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _permissions;

    public RequirePermissionAttribute(params string[] permission)
    {
        _permissions = permission;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (!user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var permissionClaims = user.FindAll("permission").Select(c => c.Value).ToList();
        if (_permissions.Any(p => permissionClaims.Contains(p)))
        {
            return;
        }

        // Fallback to service check
        var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var hasAnyPermission = _permissions.Any(p => permissionService.UserHasPermissionAsync(userId, p).Result);
        if (!hasAnyPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}
