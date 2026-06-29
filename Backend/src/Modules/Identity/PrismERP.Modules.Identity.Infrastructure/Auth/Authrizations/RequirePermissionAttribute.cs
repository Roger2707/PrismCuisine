using Microsoft.AspNetCore.Authorization;

namespace PrismERP.Modules.Identity.Infrastructure.Auth.Authrizations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = permission;
    }
}
