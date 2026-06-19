using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Inventory.Application.Inventory;
using PrismERP.Modules.Inventory.Application.Inventory.Admin;
using PrismERP.Modules.Inventory.Application.Inventory.Workflows;
using PrismERP.Modules.Inventory.Domain.Entities;

namespace PrismERP.Modules.Inventory.Infrastructure.Persistence;

public interface IInventoryDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

internal sealed class InventoryDataSeeder(
    PrismERPDbContext db,
    IInventoryBalanceAdminService balanceAdminService,
    IInventoryManualStockAdminService manualStockAdminService) : IInventoryDataSeeder
{
    private const string WarehouseCode = "MAIN";

    private static readonly ProductSeed[] Products =
    [
        new("P001", "Rau muống", "KG", 20m, 15_000m, 15m, 18_000m),
        new("P002", "Thịt ba chỉ", "KG", 10m, 120_000m, 8m, 125_000m),
        new("P003", "Tôm sú", "KG", 5m, 350_000m, 5m, 380_000m),
        new("P004", "Coca Cola", "THUNG", 12m, 180_000m, 8m, 190_000m),
        new("P005", "Nước mắm Phú Quốc", "CHAI", 6m, 45_000m, 4m, 48_000m)
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.Products.AnyAsync(p => p.Sku == Products[0].Sku, cancellationToken))
        {
            return;
        }

        var category = await EnsureCategoryAsync(cancellationToken);
        var warehouse = await EnsureWarehouseAsync(cancellationToken);

        foreach (var seed in Products)
        {
            await SeedProductStockAsync(category.Id, warehouse.Id, seed, cancellationToken);
        }
    }

    private async Task<ProductCategory> EnsureCategoryAsync(CancellationToken cancellationToken)
    {
        var category = await db.ProductCategories
            .FirstOrDefaultAsync(c => c.Code == "NGUYEN-LIEU", cancellationToken);

        if (category is not null)
        {
            return category;
        }

        category = ProductCategory.Create("NGUYEN-LIEU", "Nguyên liệu", "Nguyên liệu nhà hàng");
        db.ProductCategories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return category;
    }

    private async Task<Warehouse> EnsureWarehouseAsync(CancellationToken cancellationToken)
    {
        var warehouse = await db.Warehouses
            .FirstOrDefaultAsync(w => w.Code == WarehouseCode, cancellationToken);

        if (warehouse is not null)
        {
            return warehouse;
        }

        warehouse = Warehouse.Create(WarehouseCode, "Kho chính", "Kho bếp trung tâm");
        db.Warehouses.Add(warehouse);
        await db.SaveChangesAsync(cancellationToken);
        return warehouse;
    }

    private async Task SeedProductStockAsync(
        int categoryId,
        int warehouseId,
        ProductSeed seed,
        CancellationToken cancellationToken)
    {
        var product = Product.Create(categoryId, seed.Sku, seed.Name, seed.Unit, $"Seed product {seed.Sku}");
        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        await balanceAdminService.EnsureBalanceAsync(
            new CreateInventoryBalanceRequest(product.Id, warehouseId, 5m),
            cancellationToken);

        await manualStockAdminService.ReceiveAsync(
            new ReceiveInventoryRequest(
                product.Id,
                warehouseId,
                seed.Layer1Qty,
                seed.Layer1Cost,
                "SEED-L1",
                Notes: $"Seed layer 1 - {seed.Sku}"),
            cancellationToken);

        await manualStockAdminService.ReceiveAsync(
            new ReceiveInventoryRequest(
                product.Id,
                warehouseId,
                seed.Layer2Qty,
                seed.Layer2Cost,
                "SEED-L2",
                Notes: $"Seed layer 2 - {seed.Sku}"),
            cancellationToken);
    }

    private sealed record ProductSeed(
        string Sku,
        string Name,
        string Unit,
        decimal Layer1Qty,
        decimal Layer1Cost,
        decimal Layer2Qty,
        decimal Layer2Cost);
}
