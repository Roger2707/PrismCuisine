using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace PrismCuisine.BuildingBlocks.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddBuildingBlocksApplication(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assemblies);
        });

        services.AddValidatorsFromAssemblies(assemblies);

        return services;
    }
}
