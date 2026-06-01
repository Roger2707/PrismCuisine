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

        if (order is null)
        {
            return null;
        }

        var lines = order.Lines
            .Select(l => new PurchaseOrderLineDto(l.ProductId, l.Quantity, l.UnitPrice))
            .ToList();

        return new PurchaseOrderDto(
            order.Id,
            order.OrderNumber,
            order.Status.ToString(),
            order.PostedAt,
            lines);
    }

    public void Add(PurchaseOrder order) => db.PurchaseOrders.Add(order);

    public void Update(PurchaseOrder order) => db.PurchaseOrders.Update(order);
}
