using Microsoft.AspNetCore.Authorization;

namespace PrismERP.Modules.Identity.Infrastructure.Auth.Authrizations;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
