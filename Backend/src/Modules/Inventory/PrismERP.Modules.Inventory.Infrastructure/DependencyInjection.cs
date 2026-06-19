using Microsoft.Extensions.DependencyInjection;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Application.Inventory.Admin;
using PrismERP.Modules.Inventory.Application.Inventory.Internal;
using PrismERP.Modules.Inventory.Application.Inventory.Queries;
using PrismERP.Modules.Inventory.Application.Inventory.Workflows;
using PrismERP.Modules.Inventory.Application.ProductCategories;
using PrismERP.Modules.Inventory.Application.Products;
using PrismERP.Modules.Inventory.Application.Warehouses;
using PrismERP.Modules.Inventory.Infrastructure.Persistence;

namespace PrismERP.Modules.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        services.AddSingleton<IModulePersistenceConfigurator, InventoryPersistenceConfigurator>();
        services.AddScoped<IInventoryUnitOfWork, InventoryUnitOfWork>();
        services.AddScoped<IProductCategoryRepository>(sp => sp.GetRequiredService<IInventoryUnitOfWork>().ProductCategories);
        services.AddScoped<IProductRepository>(sp => sp.GetRequiredService<IInventoryUnitOfWork>().Products);
        services.AddScoped<IWarehouseRepository>(sp => sp.GetRequiredService<IInventoryUnitOfWork>().Warehouses);
        services.AddScoped<IInventoryBalanceRepository>(sp => sp.GetRequiredService<IInventoryUnitOfWork>().Balances);
        services.AddScoped<IInventoryMovementRepository>(sp => sp.GetRequiredService<IInventoryUnitOfWork>().Movements);
        services.AddScoped<IInventoryCostLayerRepository>(sp => sp.GetRequiredService<IInventoryUnitOfWork>().CostLayers);
        services.AddScoped<IInventoryReservationRepository>(sp => sp.GetRequiredService<IInventoryUnitOfWork>().Reservations);

        services.AddScoped<InventoryBalanceAccess>();
        services.AddScoped<InventoryAvailabilityChecker>();
        services.AddScoped<InventoryFifoIssuer>();

        services.AddScoped<IInventoryQueryService, InventoryQueryService>();
        services.AddScoped<IInventoryBalanceAdminService, InventoryBalanceAdminService>();
        services.AddScoped<IInventoryManualStockAdminService, InventoryManualStockAdminService>();
        services.AddScoped<IInventoryReservationAdminService, InventoryReservationAdminService>();
        services.AddScoped<IInventorySalesReservationWorkflowService, InventorySalesReservationWorkflowService>();
        services.AddScoped<IInventoryReceivingWorkflowService, InventoryReceivingWorkflowService>();

        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IInventoryDataSeeder, InventoryDataSeeder>();

        return services;
    }
}
