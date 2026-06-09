using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrismERP.BuildingBlocks.Application.Abstractions.Caching;
using PrismERP.BuildingBlocks.Infrastructure.Caching;
using PrismERP.BuildingBlocks.Infrastructure.Messaging;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;

namespace PrismERP.BuildingBlocks.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBuildingBlocksInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureMassTransit = null)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

        services.AddDbContext<PrismERPDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
            });
        });

        var redisConnection = configuration.GetConnectionString("Redis")
            ?? "localhost:6379";

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "PrismERP:";
        });

        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddScoped<Application.Abstractions.Messaging.IIntegrationEventPublisher, IntegrationEventPublisher>();

        var rabbitHost = configuration.GetConnectionString("RabbitMq") ?? "localhost";
        var rabbitUser = configuration["RabbitMq:Username"] ?? "guest";
        var rabbitPass = configuration["RabbitMq:Password"] ?? "guest";

        services.AddMassTransit(bus =>
        {
            configureMassTransit?.Invoke(bus);

            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitHost, "/", h =>
                {
                    h.Username(rabbitUser);
                    h.Password(rabbitPass);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
