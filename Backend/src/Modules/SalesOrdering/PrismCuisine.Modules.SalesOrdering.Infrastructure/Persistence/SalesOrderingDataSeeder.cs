using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Application.Inventory;
using PrismCuisine.Modules.Inventory.Domain.Entities;
using PrismCuisine.Modules.Inventory.Domain.Enums;
using PrismCuisine.Modules.SalesOrdering.Domain.Entities;

namespace PrismCuisine.Modules.SalesOrdering.Infrastructure.Persistence;

public interface ISalesOrderingDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

internal sealed class SalesOrderingDataSeeder(
    PrismCuisineDbContext db,
    IInventoryPostingService inventoryPosting) : ISalesOrderingDataSeeder
{
    private const string SeedMarker = "SO-SEED-001";
    private const int WarehouseId = 1;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.SalesOrders.AnyAsync(o => o.OrderNumber == SeedMarker, cancellationToken))
        {
            return;
        }

        var products = await db.Products
            .Where(p => p.Sku.StartsWith("P00"))
            .ToDictionaryAsync(p => p.Sku, cancellationToken);

        if (products.Count < 5)
        {
            return;
        }

        var customers = await SeedCustomersAsync(cancellationToken);

        await SeedSalesOrdersAsync(customers, products, cancellationToken);
    }

    private async Task<Dictionary<string, Customer>> SeedCustomersAsync(CancellationToken cancellationToken)
    {
        var seeds =
            new (string Code, string Name, string Phone)[]
            {
                ("KH-001", "Nhà hàng Sen Vàng", "0902000001"),
                ("KH-002", "Quán Cơm Niêu", "0902000002"),
                ("KH-003", "Khách sạn Riverside", "0902000003")
            };

        foreach (var seed in seeds)
        {
            if (await db.Customers.AnyAsync(c => c.Code == seed.Code, cancellationToken))
            {
                continue;
            }

            db.Customers.Add(Customer.Create(seed.Code, seed.Name, seed.Phone));
        }

        await db.SaveChangesAsync(cancellationToken);

        return await db.Customers
            .Where(c => c.Code.StartsWith("KH-"))
            .ToDictionaryAsync(c => c.Code, cancellationToken);
    }

    private async Task SeedSalesOrdersAsync(
        Dictionary<string, Customer> customers,
        Dictionary<string, Product> products,
        CancellationToken cancellationToken)
    {
        var p = products;
        var c = customers;

        var so1 = CreateSo("SO-SEED-001", c["KH-001"], "Đơn nháp - bàn tiệc nhỏ");
        AddLine(so1, p["P001"], 5m, 25_000m, 0m, 8m);
        AddLine(so1, p["P002"], 3m, 150_000m, 5m, 8m);

        var so2 = CreateSo("SO-SEED-002", c["KH-001"], "Đơn nháp - combo hải sản");
        AddLine(so2, p["P003"], 2m, 400_000m, 0m, 8m);
        AddLine(so2, p["P004"], 4m, 200_000m, 0m, 10m);
        AddLine(so2, p["P005"], 2m, 50_000m, 0m, 8m);

        var so3 = CreateSo("SO-SEED-003", c["KH-002"], "Đã xác nhận - buffet sáng");
        AddLine(so3, p["P001"], 4m, 25_000m, 0m, 8m);
        AddLine(so3, p["P003"], 2m, 400_000m, 10m, 8m);

        var so4 = CreateSo("SO-SEED-004", c["KH-002"], "Đã xác nhận - tiệc công ty");
        AddLine(so4, p["P002"], 2m, 150_000m, 0m, 8m);
        AddLine(so4, p["P004"], 3m, 200_000m, 0m, 10m);
        AddLine(so4, p["P005"], 1m, 50_000m, 0m, 8m);

        var so5 = CreateSo("SO-SEED-005", c["KH-003"], "Giao một phần - sự kiện");
        AddLine(so5, p["P001"], 5m, 25_000m, 0m, 8m);
        AddLine(so5, p["P002"], 3m, 150_000m, 0m, 8m);

        var orders = new[] { so1, so2, so3, so4, so5 };
        db.SalesOrders.AddRange(orders);
        await db.SaveChangesAsync(cancellationToken);

        await ApproveAndReserveAsync(so3, cancellationToken);
        await ApproveAndReserveAsync(so4, cancellationToken);
        await SeedPartialDeliveryAsync(so5, cancellationToken);
    }

    private static SalesOrder CreateSo(string orderNumber, Customer customer, string? notes) =>
        SalesOrder.CreateDraft(orderNumber, customer.Id, customer.Name, notes);

    private static void AddLine(
        SalesOrder order,
        Product product,
        decimal qty,
        decimal unitPrice,
        decimal discountPercent,
        decimal vatRate) =>
        order.AddLine(product.Id, product.Name, qty, unitPrice, discountPercent, vatRate);

    private async Task ApproveAndReserveAsync(SalesOrder salesOrder, CancellationToken cancellationToken)
    {
        foreach (var line in salesOrder.Lines)
        {
            await inventoryPosting.ReserveAsync(
                new CreateReservationRequest(
                    line.ProductId,
                    WarehouseId,
                    line.QuantityOrdered,
                    line.Id,
                    $"Seed reservation for {salesOrder.OrderNumber}, line {line.Id}"),
                cancellationToken);
        }

        salesOrder.Approve();
        db.SalesOrders.Update(salesOrder);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedPartialDeliveryAsync(SalesOrder salesOrder, CancellationToken cancellationToken)
    {
        await ApproveAndReserveAsync(salesOrder, cancellationToken);

        var lines = salesOrder.Lines.OrderBy(l => l.Id).ToList();

        var deliveryNote = DeliveryNote.CreateDraft(
            "DN-SEED-001",
            salesOrder.Id,
            salesOrder.CustomerId,
            salesOrder.CustomerName,
            salesOrder.OrderNumber,
            salesOrder.Status,
            "Giao một phần - seed");

        deliveryNote.AddLine(2m, lines[0]);
        deliveryNote.AddLine(1m, lines[1]);

        db.DeliveryNotes.Add(deliveryNote);
        await db.SaveChangesAsync(cancellationToken);

        var deliveryLineIds = deliveryNote.Lines.Select(l => l.SalesOrderLineId).ToHashSet();
        var reservations = await inventoryPosting.GetActivesByReferencesAsync(
            InventoryReferenceType.SalesOrder,
            deliveryLineIds,
            cancellationToken)
            ?? [];

        var fulfillLines = deliveryNote.Lines.Select(dnLine =>
        {
            var reservation = reservations.First(r => r.ReferenceId == dnLine.SalesOrderLineId);
            return new FulfillReservationLine(
                reservation,
                dnLine.QuantityDelivered,
                deliveryNote.DeliveryNumber,
                $"Seed delivery {deliveryNote.DeliveryNumber}, line {dnLine.SalesOrderLineId}");
        }).ToList();

        await inventoryPosting.FulfillReservationsAsync(fulfillLines, cancellationToken);

        deliveryNote.Post(salesOrder);
        db.DeliveryNotes.Update(deliveryNote);
        db.SalesOrders.Update(salesOrder);
        await db.SaveChangesAsync(cancellationToken);
    }
}
