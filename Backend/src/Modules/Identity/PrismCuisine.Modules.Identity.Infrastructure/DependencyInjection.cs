using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Identity.Application.Abstractions.Persistence;
using PrismCuisine.Modules.Identity.Application.Abstractions.Services;
using PrismCuisine.Modules.Identity.Application.Auth;
using PrismCuisine.Modules.Identity.Application.Users;
using PrismCuisine.Modules.Identity.Infrastructure.Auth;
using PrismCuisine.Modules.Identity.Infrastructure.Persistence;
using System.Text;

namespace PrismCuisine.Modules.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        

        services.AddSingleton<IModulePersistenceConfigurator, IdentityPersistenceConfigurator>();
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();
        services.AddScoped<IUserRepository>(sp => sp.GetRequiredService<IIdentityUnitOfWork>().Users);
        services.AddScoped<IRefreshTokenRepository>(sp => sp.GetRequiredService<IIdentityUnitOfWork>().RefreshTokens);
        services.AddScoped<IIdentityAuthorizationRepository>(sp => sp.GetRequiredService<IIdentityUnitOfWork>().Authorization);
        services.AddScoped<Pbkdf2PasswordHasher>();
        services.AddScoped<IPasswordHasher>(sp => sp.GetRequiredService<Pbkdf2PasswordHasher>());
        services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();
        services.AddScoped<IIdentityAuthService, IdentityAuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IIdentityDataSeeder, IdentityDataSeeder>();

        return services;
    }

    public static IApplicationBuilder UseIdentityAuthBlacklistUsers(this IApplicationBuilder app) =>
        app.UseMiddleware<BlockUserBlacklistMiddleware>();

    public static IApplicationBuilder UseIdentityPermissions(this IApplicationBuilder app) =>
        app.UseMiddleware<PermissionsEnrichmentMiddleware>();
}
