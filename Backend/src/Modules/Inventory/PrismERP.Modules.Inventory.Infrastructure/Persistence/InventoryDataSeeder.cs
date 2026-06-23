using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Inventory.Application.Inventory;
using PrismERP.Modules.Inventory.Application.Inventory.Admin;
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
    private const string SeedMarkerSku = "ELE-001";
    private const string WarehouseCode = "MAIN";
    private const int WarehouseId = 1;

    private static readonly ProductSeed[] Products =
    [
        // In-stock items (2 FIFO cost layers each) — for SO approve / DN post testing
        new("ELE-001", "Wireless Bluetooth Earbuds", "PCS", 120m, 18.50m, 80m, 19.00m),
        new("ELE-002", "USB-C Fast Charger 65W", "PCS", 200m, 22.00m, 150m, 23.50m),
        new("ELE-003", "Mechanical Gaming Keyboard", "PCS", 60m, 45.00m, 40m, 47.00m),
        new("ELE-004", "27-inch IPS Monitor", "PCS", 35m, 185.00m, 25m, 192.00m),
        new("ELE-005", "Portable SSD 1TB", "PCS", 90m, 72.00m, 60m, 75.00m),
        new("ELE-006", "Smart Watch Series X", "PCS", 70m, 129.00m, 50m, 135.00m),
        new("ELE-007", "Wireless Optical Mouse", "PCS", 250m, 12.00m, 180m, 13.00m),
        new("ELE-008", "HDMI 2.1 Cable 2m", "PCS", 300m, 8.50m, 200m, 9.00m),
        new("ELE-009", "Aluminum Laptop Stand", "PCS", 100m, 28.00m, 80m, 29.50m),
        new("ELE-010", "Webcam 1080p", "PCS", 85m, 35.00m, 65m, 36.50m),
        new("ELE-011", "Noise Cancelling Headphones", "PCS", 55m, 89.00m, 45m, 92.00m),
        new("ELE-012", "Power Bank 20000mAh", "PCS", 140m, 31.00m, 100m, 32.50m),
        // Balance only (zero on-hand) — for PO receive testing
        new("ELE-013", "Wi-Fi 6 Router", "PCS", 0m, 0m, 0m, 0m),
        new("ELE-014", "Smart Speaker", "PCS", 0m, 0m, 0m, 0m),
        new("ELE-015", "10-inch Tablet", "PCS", 0m, 0m, 0m, 0m),
        new("ELE-016", "Graphics Drawing Tablet", "PCS", 0m, 0m, 0m, 0m),
        new("ELE-017", "USB Condenser Microphone", "PCS", 0m, 0m, 0m, 0m),
        new("ELE-018", "USB-C Docking Station", "PCS", 0m, 0m, 0m, 0m),
        new("ELE-019", "RGB Smart Bulb", "PCS", 0m, 0m, 0m, 0m),
        new("ELE-020", "Fitness Tracker Band", "PCS", 0m, 0m, 0m, 0m),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.Products.AnyAsync(p => p.Sku == SeedMarkerSku, cancellationToken))
        {
            return;
        }

        var category = await EnsureCategoryAsync(cancellationToken);
        var warehouse = await EnsureWarehouseAsync(cancellationToken);

        foreach (var seed in Products)
        {
            await SeedProductAsync(category.Id, warehouse.Id, seed, cancellationToken);
        }
    }

    private async Task<ProductCategory> EnsureCategoryAsync(CancellationToken cancellationToken)
    {
        var category = await db.ProductCategories
            .FirstOrDefaultAsync(c => c.Code == "ELECTRONICS", cancellationToken);

        if (category is not null)
        {
            return category;
        }

        category = ProductCategory.Create(
            "ELECTRONICS",
            "Electronics",
            "Consumer electronics and accessories");
        db.ProductCategories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return category;
    }

    private async Task<Warehouse> EnsureWarehouseAsync(CancellationToken cancellationToken)
    {
        var warehouse = await db.Warehouses
            .FirstOrDefaultAsync(w => w.Id == WarehouseId || w.Code == WarehouseCode, cancellationToken);

        if (warehouse is not null)
        {
            return warehouse;
        }

        warehouse = Warehouse.Create(WarehouseCode, "Main Warehouse", "Primary electronics warehouse");
        db.Warehouses.Add(warehouse);
        await db.SaveChangesAsync(cancellationToken);
        return warehouse;
    }

    private async Task SeedProductAsync(
        int categoryId,
        int warehouseId,
        ProductSeed seed,
        CancellationToken cancellationToken)
    {
        var product = Product.Create(categoryId, seed.Sku, seed.Name, seed.Unit, $"Seed product {seed.Sku}");
        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        await balanceAdminService.EnsureBalanceAsync(
            new CreateInventoryBalanceRequest(product.Id, warehouseId, 10m),
            cancellationToken);

        if (seed.Layer1Qty <= 0)
        {
            return;
        }

        await manualStockAdminService.ReceiveAsync(
            new ReceiveInventoryRequest(
                product.Id,
                warehouseId,
                seed.Layer1Qty,
                seed.Layer1Cost,
                "SEED-L1",
                Notes: $"Seed cost layer 1 - {seed.Sku}"),
            cancellationToken);

        if (seed.Layer2Qty > 0)
        {
            await manualStockAdminService.ReceiveAsync(
                new ReceiveInventoryRequest(
                    product.Id,
                    warehouseId,
                    seed.Layer2Qty,
                    seed.Layer2Cost,
                    "SEED-L2",
                    Notes: $"Seed cost layer 2 - {seed.Sku}"),
                cancellationToken);
        }
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
