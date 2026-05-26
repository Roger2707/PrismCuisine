using Microsoft.Extensions.DependencyInjection;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Purchasing.Application.Abstractions;
using PrismCuisine.Modules.Purchasing.Infrastructure.Persistence;

namespace PrismCuisine.Modules.Purchasing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPurchasingModule(this IServiceCollection services)
    {
        services.AddSingleton<IModulePersistenceConfigurator, PurchasingPersistenceConfigurator>();
        services.AddScoped<IPurchasingUnitOfWork, PurchasingUnitOfWork>();
        services.AddScoped<IPurchaseOrderRepository>(sp => sp.GetRequiredService<IPurchasingUnitOfWork>().PurchaseOrders);
        return services;
    }
}
