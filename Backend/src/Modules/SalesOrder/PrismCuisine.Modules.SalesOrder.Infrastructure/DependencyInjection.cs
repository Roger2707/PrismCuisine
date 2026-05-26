using Microsoft.Extensions.DependencyInjection;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.SalesOrder.Infrastructure.Persistence;

namespace PrismCuisine.Modules.SalesOrder.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSalesOrderModule(this IServiceCollection services)
    {
        services.AddSingleton<IModulePersistenceConfigurator, SalesOrderPersistenceConfigurator>();
        return services;
    }
}
