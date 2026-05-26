using Microsoft.Extensions.DependencyInjection;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Infrastructure.Persistence;

namespace PrismCuisine.Modules.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        services.AddSingleton<IModulePersistenceConfigurator, InventoryPersistenceConfigurator>();
        return services;
    }
}
