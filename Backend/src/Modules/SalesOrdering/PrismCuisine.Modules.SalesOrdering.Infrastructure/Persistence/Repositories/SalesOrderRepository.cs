using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.SalesOrdering.Application.Abtractions;
using PrismCuisine.Modules.SalesOrdering.Application.SalesOrders;
using PrismCuisine.Modules.SalesOrdering.Domain.Entities;

namespace PrismCuisine.Modules.SalesOrdering.Infrastructure.Persistence.Repositories;
internal sealed class SalesOrderRepository(PrismCuisineDbContext db) : ISalesOrderRepository
{
    public async Task<IReadOnlyCollection<SalesOrderSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await db.SalesOrders
            .AsNoTracking()
            .Include(o => o.Lines)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(MapSummary).ToList();
    }

    public async Task<SalesOrderDto?> GetByIdWithLinesAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await db.SalesOrders
            .AsNoTracking()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return order is null ? null : Map(order);
    }

    public async Task<SalesOrder?> GetByIdWithLinesForUpdateAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await db.SalesOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return order;
    }

    public Task<int> GetCountForDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        return db.SalesOrders.CountAsync(o => o.CreatedAt >= start && o.CreatedAt < end, cancellationToken);
    }
    public void Add(SalesOrder order) => db.SalesOrders.Add(order);

    public void Update(SalesOrder order) => db.SalesOrders.Update(order);

    #region Mappings

    private static SalesOrderSummaryDto MapSummary(SalesOrder order)
    {
        return new SalesOrderSummaryDto(
            order.Id,
            order.OrderNumber,
            order.CustomerId,
            order.CustomerName,
            order.OrderDate,
            order.DeliveryDate,
            order.ApprovedAt,
            order.Status.ToString(),
            order.Notes,
            order.SubTotal,
            order.TotalDiscount,
            order.TotalVAT,
            order.TotalAmount
        );
    }

    private static SalesOrderDto Map(SalesOrder order)
    {
        var lines = order.Lines
            .Select(l => new SalesOrderLineDto(
                l.Id,
                l.ProductId,
                l.ProductName,
                l.QuantityOrdered,
                l.QuantityDelivered,
                l.QuantityRemaining,
                l.UnitPrice,
                l.DiscountPercent,
                l.VATRate,
                l.DiscountAmount,
                l.VATAmount,
                l.LineTotal
                ))
            .ToList();

        return new SalesOrderDto(
            order.Id,
            order.OrderNumber,
            order.CustomerId,
            order.CustomerName,
            order.OrderDate,
            order.DeliveryDate,
            order.ApprovedAt,
            order.Status.ToString(),
            order.Notes,
            order.SubTotal,
            order.TotalDiscount,
            order.TotalVAT,
            order.TotalAmount,
            lines
        );
    }

    #endregion
}