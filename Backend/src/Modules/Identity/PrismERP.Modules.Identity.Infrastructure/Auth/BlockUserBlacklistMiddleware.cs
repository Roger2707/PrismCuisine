using Microsoft.AspNetCore.Http;
using PrismERP.BuildingBlocks.Application.Abstractions.Caching;
using System.Security.Claims;

namespace PrismERP.Modules.Identity.Infrastructure.Auth;

public sealed class BlockUserBlacklistMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICacheService cacheService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                string cacheKey = $"blacklist:user:{userId}";
                bool isBlacklisted = await cacheService.ExistsAsync(cacheKey);
                if (isBlacklisted)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    context.Response.Headers.Append("WWW-Authenticate", "Bearer error=\"invalid_token\", error_description=\"The token is expired or revoked.\"");

                    var responseError = new
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = "Your seesion account is logged out or blocked!"
                    };

                    await context.Response.WriteAsJsonAsync(responseError);
                    return;
                }
            }
        }
        await next(context);
    }
}
