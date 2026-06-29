using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using PrismERP.Modules.Identity.Application.Authorization;

namespace PrismERP.Modules.Identity.Infrastructure.Auth.Authrizations;

public static class IdentityAuthorizationExtensions
{
    public const string ForbiddenMessage = "You do not have permission to perform this action.";

    public static IServiceCollection AddIdentityAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            foreach (var permission in PermissionCodes.All)
            {
                options.AddPolicy(permission, policy =>
                    policy.Requirements.Add(new PermissionRequirement(permission)));
            }
        });

        return services;
    }
}
