using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace PrismERP.Modules.Identity.Infrastructure.Auth.Authrizations;

public sealed class PermissionAuthorizationHandler(IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
            return Task.CompletedTask;

        if (httpContext.Items.TryGetValue("permissions", out var cachedPermissions))
        {
            // case 1: SuperAdmin
            if (cachedPermissions is string stringPerm && stringPerm == "*")
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // case 2: verify is permissions contains requirement 
            if (cachedPermissions is IEnumerable<string> permissions
                && permissions.Contains(requirement.Permission, StringComparer.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
