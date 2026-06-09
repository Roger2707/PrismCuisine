using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Inventory.Application.Inventory;
using PrismERP.Modules.Inventory.Domain.Entities;
using PrismERP.Modules.Purchasing.Domain.Entities;
using PrismERP.Modules.Purchasing.Domain.Enums;

namespace PrismERP.Modules.Purchasing.Infrastructure.Persistence;

public interface IPurchasingDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

internal sealed class PurchasingDataSeeder(
    PrismERPDbContext db,
    IInventoryPostingService inventoryPosting) : IPurchasingDataSeeder
{
    private const string SeedMarker = "PO-SEED-001";

    private static readonly SupplierSeed[] Suppliers =
    [
        new("NCC-RAU", "Hợp tác xã Rau sạch Đà Lạt", "0901000001", "rausach@example.com", "Chợ đầu mối Hóc Môn"),
        new("NCC-THIT", "Công ty Thực phẩm An Bình", "0901000002", "anbinh@example.com", "KCN Bình Chánh"),
        new("NCC-HAISAN", "Hải sản Nha Trang Fresh", "0901000003", "fresh@example.com", "Chợ Bình Đông"),
        new("NCC-DOUONG", "Phân phối Coca & nước giải khát", "0901000004", "coca@example.com", "Quận 7, TP.HCM"),
        new("NCC-GIAVI", "Gia vị Việt Nam Foods", "0901000005", "giaivi@example.com", "Bình Thạnh"),
        new("NCC-BAOBI", "Bao bì Xanh Packaging", "0901000006", "pack@example.com", "Thủ Đức")
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedSuppliersAsync(cancellationToken);

        if (await db.PurchaseOrders.AnyAsync(o => o.OrderNumber == SeedMarker, cancellationToken))
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

        var suppliers = await db.Suppliers.ToDictionaryAsync(s => s.Code, cancellationToken);
        var warehouseId = await db.Warehouses
            .Where(w => w.Code == "MAIN")
            .Select(w => w.Id)
            .FirstAsync(cancellationToken);

        await SeedPurchaseOrdersAsync(products, suppliers, warehouseId, cancellationToken);
    }

    private async Task SeedSuppliersAsync(CancellationToken cancellationToken)
    {
        foreach (var seed in Suppliers)
        {
            var exists = await db.Suppliers.AnyAsync(s => s.Code == seed.Code, cancellationToken);
            if (exists)
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

    private async Task SeedPurchaseOrdersAsync(
        Dictionary<string, Product> products,
        Dictionary<string, Supplier> suppliers,
        int warehouseId,
        CancellationToken cancellationToken)
    {
        var p = products;

        var po1 = CreatePo("PO-SEED-001", suppliers["NCC-RAU"].Id, warehouseId, "Draft - rau & gia vị");
        po1.AddLine(p["P001"].Id, 30m, 16_000m);
        po1.AddLine(p["P005"].Id, 10m, 46_000m);

        var po2 = CreatePo("PO-SEED-002", suppliers["NCC-THIT"].Id, warehouseId, "Draft - thịt");
        po2.AddLine(p["P002"].Id, 20m, 122_000m);

        var po3 = CreatePo("PO-SEED-003", suppliers["NCC-HAISAN"].Id, warehouseId, "Approved - tôm");
        po3.AddLine(p["P003"].Id, 15m, 360_000m);
        po3.Approve();

        var po4 = CreatePo("PO-SEED-004", suppliers["NCC-DOUONG"].Id, warehouseId, "Approved - nước ngọt");
        po4.AddLine(p["P004"].Id, 24m, 185_000m);
        po4.Approve();

        var po5 = CreatePo("PO-SEED-005", suppliers["NCC-RAU"].Id, warehouseId, "Partially received - rau");
        po5.AddLine(p["P001"].Id, 10m, 17_000m);
        po5.Approve();

        var po6 = CreatePo("PO-SEED-006", suppliers["NCC-GIAVI"].Id, warehouseId, "Received - nước mắm");
        po6.AddLine(p["P005"].Id, 8m, 47_000m);
        po6.Approve();

        var po7 = CreatePo("PO-SEED-007", suppliers["NCC-BAOBI"].Id, warehouseId, "Cancelled");
        po7.AddLine(p["P004"].Id, 5m, 190_000m);
        po7.Cancel();

        var po8 = CreatePo("PO-SEED-008", suppliers["NCC-THIT"].Id, warehouseId, "Approved - thịt nhiều");
        po8.AddLine(p["P002"].Id, 5m, 123_000m);
        po8.AddLine(p["P003"].Id, 3m, 365_000m);
        po8.Approve();

        var orders = new[] { po1, po2, po3, po4, po5, po6, po7, po8 };
        db.PurchaseOrders.AddRange(orders);
        await db.SaveChangesAsync(cancellationToken);

        await SeedGoodsReceiptsAsync(po5, po6, warehouseId, cancellationToken);
    }

    private async Task SeedGoodsReceiptsAsync(
        PurchaseOrder partialPo,
        PurchaseOrder receivedPo,
        int warehouseId,
        CancellationToken cancellationToken)
    {
        var partialLine = partialPo.Lines.First();
        var grPartial = GoodsReceipt.CreateDraft(partialPo.Id, "GRN-SEED-001", "Nhận một phần rau");
        grPartial.AddLine(partialLine.Id, partialLine.ProductId, 6m, 17_000m);
        db.GoodsReceipts.Add(grPartial);
        await db.SaveChangesAsync(cancellationToken);
        await PostGoodsReceiptAsync(grPartial, partialPo, warehouseId, cancellationToken);

        var receivedLine = receivedPo.Lines.First();
        var grFull = GoodsReceipt.CreateDraft(receivedPo.Id, "GRN-SEED-002", "Nhận đủ nước mắm");
        grFull.AddLine(receivedLine.Id, receivedLine.ProductId, receivedLine.QuantityOrdered, 47_000m);
        db.GoodsReceipts.Add(grFull);
        await db.SaveChangesAsync(cancellationToken);
        await PostGoodsReceiptAsync(grFull, receivedPo, warehouseId, cancellationToken);
    }

    private async Task PostGoodsReceiptAsync(
        GoodsReceipt receipt,
        PurchaseOrder order,
        int warehouseId,
        CancellationToken cancellationToken)
    {
        foreach (var line in receipt.Lines)
        {
            order.RecordReceipt(line.PurchaseOrderLineId, line.Quantity);

            await inventoryPosting.ReceiveAsync(
                new ReceiveInventoryRequest(
                    line.ProductId,
                    warehouseId,
                    line.Quantity,
                    line.UnitCost,
                    receipt.ReceiptNumber,
                    receipt.PurchaseOrderId,
                    $"Seed GRN line {line.Id}"),
                cancellationToken);
        }

        receipt.Post();
        db.PurchaseOrders.Update(order);
        db.GoodsReceipts.Update(receipt);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static PurchaseOrder CreatePo(
        string orderNumber,
        int supplierId,
        int warehouseId,
        string? notes) =>
        PurchaseOrder.CreateDraft(orderNumber, supplierId, warehouseId, notes);

    private sealed record SupplierSeed(
        string Code,
        string Name,
        string? Phone,
        string? Email,
        string? Address,
        string? TaxCode = null);
}
