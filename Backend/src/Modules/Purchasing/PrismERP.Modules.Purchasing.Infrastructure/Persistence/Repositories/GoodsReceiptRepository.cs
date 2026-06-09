using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Purchasing.Application.Abstractions;
using PrismERP.Modules.Purchasing.Application.GoodsReceipts;
using PrismERP.Modules.Purchasing.Domain.Entities;

namespace PrismERP.Modules.Purchasing.Infrastructure.Persistence.Repositories;

internal sealed class GoodsReceiptRepository(PrismERPDbContext db) : IGoodsReceiptRepository
{
    public Task<GoodsReceipt?> GetByIdWithLinesForUpdateAsync(int id, CancellationToken cancellationToken = default) =>
        db.GoodsReceipts
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<GoodsReceiptDto?> GetByIdWithLinesAsync(int id, CancellationToken cancellationToken = default)
    {
        var receipt = await db.GoodsReceipts
            .AsNoTracking()
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        return receipt is null ? null : Map(receipt);
    }

    public async Task<IReadOnlyCollection<GoodsReceiptSummaryDto>> GetByPurchaseOrderIdAsync(
        int purchaseOrderId,
        CancellationToken cancellationToken = default) =>
        await db.GoodsReceipts
            .AsNoTracking()
            .Where(r => r.PurchaseOrderId == purchaseOrderId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new GoodsReceiptSummaryDto(
                r.Id,
                r.ReceiptNumber,
                r.PurchaseOrderId,
                r.Status.ToString(),
                r.PostedAt))
            .ToListAsync(cancellationToken);

    public Task<int> GetCountForDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        return db.GoodsReceipts.CountAsync(r => r.CreatedAt >= start && r.CreatedAt < end, cancellationToken);
    }

    public void Add(GoodsReceipt receipt) => db.GoodsReceipts.Add(receipt);

    public void Update(GoodsReceipt receipt) => db.GoodsReceipts.Update(receipt);

    private static GoodsReceiptDto Map(GoodsReceipt receipt)
    {
        var lines = receipt.Lines
            .Select(l => new GoodsReceiptLineDto(
                l.Id,
                l.PurchaseOrderLineId,
                l.ProductId,
                l.Quantity,
                l.UnitCost))
            .ToList();

        return new GoodsReceiptDto(
            receipt.Id,
            receipt.ReceiptNumber,
            receipt.PurchaseOrderId,
            receipt.Status.ToString(),
            receipt.PostedAt,
            receipt.Notes,
            lines);
    }
}
