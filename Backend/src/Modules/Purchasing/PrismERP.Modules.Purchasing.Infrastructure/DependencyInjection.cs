using Microsoft.Extensions.DependencyInjection;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Purchasing.Application.Abstractions;
using PrismERP.Modules.Purchasing.Application.GoodsReceipts;
using PrismERP.Modules.Purchasing.Application.PurchaseInvoices;
using PrismERP.Modules.Purchasing.Application.PurchaseOrders;
using PrismERP.Modules.Purchasing.Application.Suppliers;
using PrismERP.Modules.Purchasing.Infrastructure.Persistence;

namespace PrismERP.Modules.Purchasing.Infrastructure;

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
        services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
        services.AddScoped<IPurchasingDataSeeder, PurchasingDataSeeder>();

        return services;
    }
}
