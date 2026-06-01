using Microsoft.Extensions.DependencyInjection;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Purchasing.Application.Abstractions;
using PrismCuisine.Modules.Purchasing.Application.GoodsReceipts;
using PrismCuisine.Modules.Purchasing.Application.PurchaseOrders;
using PrismCuisine.Modules.Purchasing.Application.Suppliers;
using PrismCuisine.Modules.Purchasing.Infrastructure.Persistence;

namespace PrismCuisine.Modules.Purchasing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPurchasingModule(this IServiceCollection services)
    {
        services.AddSingleton<IModulePersistenceConfigurator, PurchasingPersistenceConfigurator>();
        services.AddScoped<IPurchasingUnitOfWork, PurchasingUnitOfWork>();
        services.AddScoped<ISupplierRepository>(sp => sp.GetRequiredService<IPurchasingUnitOfWork>().Suppliers);
        services.AddScoped<IPurchaseOrderRepository>(sp => sp.GetRequiredService<IPurchasingUnitOfWork>().PurchaseOrders);
        services.AddScoped<IGoodsReceiptRepository>(sp => sp.GetRequiredService<IPurchasingUnitOfWork>().GoodsReceipts);
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<IGoodsReceiptService, GoodsReceiptService>();
        services.AddScoped<IPurchasingDataSeeder, PurchasingDataSeeder>();

        return services;
    }
}
