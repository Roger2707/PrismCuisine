using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.SalesOrdering.Application.Abtractions;
using PrismCuisine.Modules.SalesOrdering.Application.Deliveries;
using PrismCuisine.Modules.SalesOrdering.Domain.Entities;

namespace PrismCuisine.Modules.SalesOrdering.Infrastructure.Persistence.Repositories;

internal sealed class DeliveryNoteRepository(PrismCuisineDbContext db) : IDeliveryNoteRepository
{
    #region Read

    public async Task<IReadOnlyCollection<DeliveryNoteSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var deliveries = await db.DeliveryNotes
            .AsNoTracking()
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return deliveries.Select(MapSummary).ToList();
    }

    public async Task<DeliveryNoteDto?> GetByIdWithLinesAsync(int id, CancellationToken cancellationToken = default)
    {
        var delivery = await db.DeliveryNotes
            .AsNoTracking()
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == id);

        return delivery is null ? null : Map(delivery);
    }

    public async Task<DeliveryNote?> GetByIdWithLinesForUpdateAsync(int id, CancellationToken cancellationToken = default)
    {
        var delivery = await db.DeliveryNotes
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == id);
        return delivery;
    }

    #endregion

    #region Write

    public void Add(DeliveryNote order) => db.DeliveryNotes.Add(order);

    public void Update(DeliveryNote order) => db.DeliveryNotes.Update(order);

    #endregion

    #region Helper Mappings

    public Task<int> GetCountForDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        return db.DeliveryNotes.CountAsync(d => d.CreatedAt >= start && d.CreatedAt < end, cancellationToken);
    }

    private static DeliveryNoteSummaryDto MapSummary(DeliveryNote delivery)
    {
        return new DeliveryNoteSummaryDto(
            delivery.Id,
            delivery.DeliveryNumber,
            delivery.SalesOrderId,
            delivery.CustomerId,
            delivery.CustomerName,
            delivery.OrderNumber,
            delivery.DeliveryDate,
            delivery.Status.ToString(),
            delivery.Notes
        );
    }

    private static DeliveryNoteDto Map(DeliveryNote delivery)
    {
        var lines = delivery.Lines
            .Select(l => new DeliveryNoteLineDto(
                l.Id,
                l.SalesOrderLineId,
                l.ProductId,
                l.ProductName,
                l.QuantityDelivered
                ))
            .ToList();

        return new DeliveryNoteDto(
            delivery.Id,
            delivery.DeliveryNumber,
            delivery.SalesOrderId,
            delivery.CustomerId,
            delivery.CustomerName,
            delivery.OrderNumber,
            delivery.DeliveryDate,
            delivery.Status.ToString(),
            delivery.Notes,
            lines
        );
    }

    #endregion
}
