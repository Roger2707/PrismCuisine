using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Inventory.Domain.Entities;
using PrismERP.Modules.SalesOrdering.Domain.Entities;

namespace PrismERP.Modules.SalesOrdering.Infrastructure.Persistence;

public interface ISalesOrderingDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

internal sealed class SalesOrderingDataSeeder(PrismERPDbContext db) : ISalesOrderingDataSeeder
{
    private const string SeedMarker = "SO-ELE-001";

    private static readonly CustomerSeed[] Customers =
    [
        new("CUS-RETAIL", "BrightTech Retail Chain", "18006661001"),
        new("CUS-ONLINE", "ElectroMart Online Store", "18006661002"),
        new("CUS-CORP", "Nexus Corporate IT Dept.", "18006661003"),
        new("CUS-EDU", "Campus Electronics Store", "18006661004"),
        new("CUS-RESELL", "Prime Gadget Resellers", "18006661005"),
    ];

    private static readonly SalesOrderSeed[] SalesOrders =
    [
        new("SO-ELE-001", "CUS-RETAIL", "Draft — store display refresh", [("ELE-001", 10m, 29.99m, 0m, 10m), ("ELE-007", 25m, 19.99m, 5m, 10m)]),
        new("SO-ELE-002", "CUS-ONLINE", "Draft — flash sale bundle", [("ELE-002", 15m, 34.99m, 0m, 10m), ("ELE-012", 20m, 49.99m, 0m, 10m), ("ELE-008", 30m, 14.99m, 0m, 10m)]),
        new("SO-ELE-003", "CUS-CORP", "Draft — office upgrade kit", [("ELE-003", 8m, 79.99m, 0m, 10m), ("ELE-004", 5m, 279.99m, 0m, 10m)]),
        new("SO-ELE-004", "CUS-EDU", "Draft — student laptop accessories", [("ELE-005", 12m, 109.99m, 0m, 10m), ("ELE-009", 18m, 39.99m, 0m, 10m)]),
        new("SO-ELE-005", "CUS-RESELL", "Draft — reseller pack A", [("ELE-006", 6m, 199.99m, 0m, 10m), ("ELE-011", 4m, 139.99m, 0m, 10m)]),
        new("SO-ELE-006", "CUS-RETAIL", "Draft — home office promo", [("ELE-010", 10m, 59.99m, 0m, 10m), ("ELE-018", 5m, 149.99m, 0m, 10m)]),
        new("SO-ELE-007", "CUS-ONLINE", "Draft — streaming starter set", [("ELE-017", 7m, 79.99m, 0m, 10m), ("ELE-010", 7m, 59.99m, 0m, 10m), ("ELE-016", 3m, 89.99m, 0m, 10m)]),
        new("SO-ELE-008", "CUS-CORP", "Draft — meeting room equipment", [("ELE-004", 3m, 279.99m, 0m, 10m), ("ELE-011", 2m, 139.99m, 0m, 10m)]),
        new("SO-ELE-009", "CUS-EDU", "Draft — lab peripherals", [("ELE-007", 40m, 19.99m, 10m, 10m), ("ELE-008", 50m, 14.99m, 0m, 10m)]),
        new("SO-ELE-010", "CUS-RESELL", "Draft — mixed shelf replenishment", [("ELE-001", 20m, 29.99m, 0m, 10m), ("ELE-002", 20m, 34.99m, 0m, 10m), ("ELE-012", 15m, 49.99m, 0m, 10m)]),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var customers = await SeedCustomersAsync(cancellationToken);

        if (await db.SalesOrders.AnyAsync(o => o.OrderNumber == SeedMarker, cancellationToken))
        {
            return;
        }

        var products = await db.Products
            .Where(p => p.Sku.StartsWith("ELE-"))
            .ToDictionaryAsync(p => p.Sku, cancellationToken);

        if (products.Count < 12)
        {
            return;
        }

        var orders = SalesOrders.Select(seed =>
        {
            var customer = customers[seed.CustomerCode];
            var order = SalesOrder.CreateDraft(
                seed.OrderNumber,
                customer.Id,
                customer.Name,
                seed.Notes);

            foreach (var line in seed.Lines)
            {
                var product = products[line.Sku];
                order.AddLine(product.Id, product.Name, line.Qty, line.UnitPrice, line.DiscountPercent, line.VatRate);
            }

            order.RecalculateTotals();
            return order;
        }).ToList();

        db.SalesOrders.AddRange(orders);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Dictionary<string, Customer>> SeedCustomersAsync(CancellationToken cancellationToken)
    {
        foreach (var seed in Customers)
        {
            if (await db.Customers.AnyAsync(c => c.Code == seed.Code, cancellationToken))
            {
                continue;
            }

            db.Customers.Add(Customer.Create(seed.Code, seed.Name, seed.Phone));
        }

        await db.SaveChangesAsync(cancellationToken);

        return await db.Customers
            .Where(c => Customers.Select(x => x.Code).Contains(c.Code))
            .ToDictionaryAsync(c => c.Code, cancellationToken);
    }

    private sealed record CustomerSeed(string Code, string Name, string Phone);

    private sealed record SalesOrderSeed(
        string OrderNumber,
        string CustomerCode,
        string Notes,
        (string Sku, decimal Qty, decimal UnitPrice, decimal DiscountPercent, decimal VatRate)[] Lines);
}
