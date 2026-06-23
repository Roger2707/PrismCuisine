using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Inventory.Domain.Entities;
using PrismERP.Modules.Purchasing.Domain.Entities;

namespace PrismERP.Modules.Purchasing.Infrastructure.Persistence;

public interface IPurchasingDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

internal sealed class PurchasingDataSeeder(PrismERPDbContext db) : IPurchasingDataSeeder
{
    private const string SeedMarker = "PO-ELE-001";
    private const int WarehouseId = 1;

    private static readonly SupplierSeed[] Suppliers =
    [
        new("SUP-TECH", "TechSource Global Ltd.", "18005551234", "orders@techsource.example.com", "1200 Silicon Valley Blvd, San Jose, CA"),
        new("SUP-GADGET", "GadgetHub Wholesale", "18005552345", "sales@gadgethub.example.com", "88 Harbor Road, Seattle, WA"),
        new("SUP-DIGI", "DigiParts Distribution Inc.", "18005553456", "procurement@digiparts.example.com", "500 Innovation Park, Austin, TX"),
        new("SUP-SMART", "SmartGear Supply Co.", "18005554567", "hello@smartgear.example.com", "77 Commerce Street, Dallas, TX"),
        new("SUP-ACC", "Accessories Direct LLC", "18005555678", "buy@accessoriesdirect.example.com", "19 Warehouse Lane, Newark, NJ"),
    ];

    private static readonly PurchaseOrderSeed[] PurchaseOrders =
    [
        new("PO-ELE-001", "SUP-TECH", "Draft restock — audio & power", [("ELE-013", 40m, 68.00m), ("ELE-014", 30m, 42.00m), ("ELE-015", 25m, 210.00m)]),
        new("PO-ELE-002", "SUP-GADGET", "Draft restock — input devices", [("ELE-016", 20m, 55.00m), ("ELE-017", 35m, 48.00m)]),
        new("PO-ELE-003", "SUP-DIGI", "Draft restock — connectivity", [("ELE-018", 15m, 95.00m), ("ELE-019", 100m, 14.50m), ("ELE-020", 60m, 22.00m)]),
        new("PO-ELE-004", "SUP-SMART", "Draft Q2 monitors & storage", [("ELE-004", 10m, 178.00m), ("ELE-005", 25m, 69.00m)]),
        new("PO-ELE-005", "SUP-ACC", "Draft accessories bundle", [("ELE-007", 80m, 11.00m), ("ELE-008", 150m, 7.80m), ("ELE-009", 40m, 26.00m)]),
        new("PO-ELE-006", "SUP-TECH", "Draft wearables replenishment", [("ELE-006", 20m, 125.00m), ("ELE-020", 50m, 21.00m)]),
        new("PO-ELE-007", "SUP-GADGET", "Draft office peripherals", [("ELE-003", 15m, 43.00m), ("ELE-010", 30m, 33.00m)]),
        new("PO-ELE-008", "SUP-DIGI", "Draft premium audio", [("ELE-001", 50m, 17.80m), ("ELE-011", 25m, 86.00m)]),
        new("PO-ELE-009", "SUP-SMART", "Draft charging & power", [("ELE-002", 60m, 21.00m), ("ELE-012", 45m, 29.00m)]),
        new("PO-ELE-010", "SUP-ACC", "Draft mixed electronics order", [("ELE-013", 12m, 70.00m), ("ELE-018", 8m, 98.00m), ("ELE-019", 40m, 15.00m)]),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedSuppliersAsync(cancellationToken);

        if (await db.PurchaseOrders.AnyAsync(o => o.OrderNumber == SeedMarker, cancellationToken))
        {
            return;
        }

        var products = await db.Products
            .Where(p => p.Sku.StartsWith("ELE-"))
            .ToDictionaryAsync(p => p.Sku, cancellationToken);

        if (products.Count < 20)
        {
            return;
        }

        var suppliers = await db.Suppliers.ToDictionaryAsync(s => s.Code, cancellationToken);
        var warehouseId = await ResolveWarehouseIdAsync(cancellationToken);

        var orders = PurchaseOrders.Select(seed =>
        {
            var po = PurchaseOrder.CreateDraft(
                seed.OrderNumber,
                suppliers[seed.SupplierCode].Id,
                warehouseId,
                seed.Notes);

            foreach (var line in seed.Lines)
            {
                po.AddLine(products[line.Sku].Id, line.Qty, line.UnitPrice);
            }

            return po;
        }).ToList();

        db.PurchaseOrders.AddRange(orders);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> ResolveWarehouseIdAsync(CancellationToken cancellationToken)
    {
        var warehouse = await db.Warehouses
            .FirstOrDefaultAsync(w => w.Id == WarehouseId || w.Code == "MAIN", cancellationToken);

        return warehouse?.Id ?? WarehouseId;
    }

    private async Task SeedSuppliersAsync(CancellationToken cancellationToken)
    {
        foreach (var seed in Suppliers)
        {
            if (await db.Suppliers.AnyAsync(s => s.Code == seed.Code, cancellationToken))
            {
                continue;
            }

            db.Suppliers.Add(Supplier.Create(
                seed.Code,
                seed.Name,
                seed.Phone,
                seed.Email,
                address: seed.Address,
                taxCode: seed.TaxCode));
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private sealed record SupplierSeed(
        string Code,
        string Name,
        string? Phone,
        string? Email,
        string? Address,
        string? TaxCode = null);

    private sealed record PurchaseOrderSeed(
        string OrderNumber,
        string SupplierCode,
        string Notes,
        (string Sku, decimal Qty, decimal UnitPrice)[] Lines);
}
