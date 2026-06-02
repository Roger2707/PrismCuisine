using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Purchasing.Application.Abstractions;
using PrismCuisine.Modules.Purchasing.Application.PurchaseOrders;
using PrismCuisine.Modules.Purchasing.Domain.Entities;

namespace PrismCuisine.Modules.Purchasing.Infrastructure.Persistence.Repositories;

internal sealed class PurchaseOrderRepository(PrismCuisineDbContext db) : IPurchaseOrderRepository
{
    public Task<PurchaseOrder?> GetByIdWithLinesForUpdateAsync(int id, CancellationToken cancellationToken = default) =>
        db.PurchaseOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<PurchaseOrderDto?> GetByIdWithLinesAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await db.PurchaseOrders
            .AsNoTracking()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return order is null ? null : Map(order);
    }

    public async Task<IReadOnlyCollection<PurchaseOrderSummaryDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var orders = await db.PurchaseOrders
            .AsNoTracking()
            .Include(o => o.Lines)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(o => new PurchaseOrderSummaryDto(
            o.Id,
            o.OrderNumber,
            o.SupplierId,
            o.WarehouseId,
            o.Status.ToString(),
            o.AmendedFromPurchaseOrderId,
            o.ApprovedAt,
            o.Lines.Sum(l => l.QuantityOrdered * l.UnitPrice))).ToList();
    }

    public Task<int> GetCountForDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        return db.PurchaseOrders.CountAsync(o => o.CreatedAt >= start && o.CreatedAt < end, cancellationToken);
    }

    public void Add(PurchaseOrder order) => db.PurchaseOrders.Add(order);

    public void Update(PurchaseOrder order) => db.PurchaseOrders.Update(order);

    private static PurchaseOrderDto Map(PurchaseOrder order)
    {
        var lines = order.Lines
            .Select(l => new PurchaseOrderLineDto(
                l.Id,
                l.ProductId,
                l.QuantityOrdered,
                l.QuantityReceived,
                l.QuantityRemaining,
                l.UnitPrice))
            .ToList();

        return new PurchaseOrderDto(
            order.Id,
            order.OrderNumber,
            order.SupplierId,
            order.WarehouseId,
            order.Status.ToString(),
            order.AmendedFromPurchaseOrderId,
            order.ApprovedAt,
            order.Notes,
            lines);
    }
}
