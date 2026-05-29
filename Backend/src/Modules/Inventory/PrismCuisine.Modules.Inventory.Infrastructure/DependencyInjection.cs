using Microsoft.Extensions.DependencyInjection;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;
using PrismCuisine.Modules.Inventory.Application.Inventory;
using PrismCuisine.Modules.Inventory.Application.ProductCategories;
using PrismCuisine.Modules.Inventory.Application.Products;
using PrismCuisine.Modules.Inventory.Application.Warehouses;
using PrismCuisine.Modules.Inventory.Infrastructure.Persistence;

namespace PrismCuisine.Modules.Inventory.Infrastructure;

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
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IInventoryPostingService, InventoryPostingService>();
        services.AddScoped<IInventoryDataSeeder, InventoryDataSeeder>();

        return services;
    }
}
