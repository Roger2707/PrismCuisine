using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PrismCuisine.Modules.Identity.Application.Abstractions.Persistence;

namespace PrismCuisine.Modules.Identity.Infrastructure.Auth;

public sealed class PermissionsEnrichmentMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IIdentityUnitOfWork unitOfWork)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");

        if (!int.TryParse(sub, out var userId))
        {
            await next(context);
            return;
        }

        if (await unitOfWork.Authorization.IsSuperAdminAsync(userId, context.RequestAborted))
        {
            context.Items["permissions"] = "*";
            await next(context);
            return;
        }

        var permissions = await unitOfWork.Authorization.GetPermissionCodesByUserIdAsync(
            userId,
            context.RequestAborted);

        context.Items["permissions"] = permissions;
        await next(context);
    }
}
