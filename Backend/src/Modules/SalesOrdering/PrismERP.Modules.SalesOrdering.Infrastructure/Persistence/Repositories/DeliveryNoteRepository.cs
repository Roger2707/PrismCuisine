using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.SalesOrdering.Application.Abtractions;
using PrismERP.Modules.SalesOrdering.Application.Deliveries;
using PrismERP.Modules.SalesOrdering.Domain.Entities;

namespace PrismERP.Modules.SalesOrdering.Infrastructure.Persistence.Repositories;

internal sealed class DeliveryNoteRepository(PrismERPDbContext db) : IDeliveryNoteRepository
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
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        return delivery is null ? null : Map(delivery);
    }

    public async Task<DeliveryNote?> GetByIdWithLinesForUpdateAsync(int id, CancellationToken cancellationToken = default)
    {
        var delivery = await db.DeliveryNotes
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        return delivery;
    }

    public async Task<List<DeliveryNote>> GetBySalesOrderIdAsync(int salesOrderId, CancellationToken cancellationToken = default)
    {
        var deliveryNotes = await db.DeliveryNotes
            .Include(d => d.Lines)
            .Where(d => d.SalesOrderId == salesOrderId)
            .ToListAsync(cancellationToken);

        return deliveryNotes;
    }

    #endregion

    #region Write

    public void Add(DeliveryNote deliveryNote) => db.DeliveryNotes.Add(deliveryNote);

    public void Update(DeliveryNote deliveryNote) => db.DeliveryNotes.Update(deliveryNote);

    public void Delete(DeliveryNote deliveryNote) => db.DeliveryNotes.Remove(deliveryNote);
    public void RemoveRange(List<DeliveryNote> deliveryNotes) => db.DeliveryNotes.RemoveRange(deliveryNotes);

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
