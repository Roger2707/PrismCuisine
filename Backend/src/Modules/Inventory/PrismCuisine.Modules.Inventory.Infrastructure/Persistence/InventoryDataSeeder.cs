using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Application.Inventory;
using PrismCuisine.Modules.Inventory.Domain.Entities;

namespace PrismCuisine.Modules.Inventory.Infrastructure.Persistence;

public interface IInventoryDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

internal sealed class InventoryDataSeeder(
    PrismCuisineDbContext db,
    IInventoryPostingService postingService) : IInventoryDataSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var category = await EnsureCategoryAsync(cancellationToken);
        var warehouse = await EnsureWarehouseAsync(cancellationToken);
        await EnsureSampleProductAsync(category.Id, warehouse.Id, cancellationToken);
    }

    private async Task<ProductCategory> EnsureCategoryAsync(CancellationToken cancellationToken)
    {
        var category = await db.ProductCategories
            .FirstOrDefaultAsync(c => c.Code == "GENERAL", cancellationToken);

        if (category is not null)
        {
            return category;
        }

        category = ProductCategory.Create("GENERAL", "General", "Default product category");
        db.ProductCategories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return category;
    }

    private async Task<Warehouse> EnsureWarehouseAsync(CancellationToken cancellationToken)
    {
        var warehouse = await db.Warehouses
            .FirstOrDefaultAsync(w => w.Code == "MAIN", cancellationToken);

        if (warehouse is not null)
        {
            return warehouse;
        }

        warehouse = Warehouse.Create("MAIN", "Main Warehouse", "Head office storage");
        db.Warehouses.Add(warehouse);
        await db.SaveChangesAsync(cancellationToken);
        return warehouse;
    }

    private async Task EnsureSampleProductAsync(
        int categoryId,
        int warehouseId,
        CancellationToken cancellationToken)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p => p.Sku == "DEMO-001", cancellationToken);

        if (product is null)
        {
            product = Product.Create(categoryId, "DEMO-001", "Demo Product", "EA", "Sample inventory item");
            db.Products.Add(product);
            await db.SaveChangesAsync(cancellationToken);
        }

        var balance = await db.InventoryBalances
            .FirstOrDefaultAsync(
                b => b.ProductId == product.Id && b.WarehouseId == warehouseId,
                cancellationToken);

        if (balance is not null && balance.QuantityOnHand > 0)
        {
            return;
        }

        await postingService.EnsureBalanceAsync(
            new CreateInventoryBalanceRequest(product.Id, warehouseId, 10m),
            cancellationToken);

        await postingService.ReceiveAsync(
            new ReceiveInventoryRequest(product.Id, warehouseId, 100m, 5.50m, "SEED", Notes: "Initial demo stock"),
            cancellationToken);
    }
}
