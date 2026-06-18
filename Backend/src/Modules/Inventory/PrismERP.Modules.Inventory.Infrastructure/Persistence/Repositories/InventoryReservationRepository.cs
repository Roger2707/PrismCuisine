using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Domain.Entities;
using PrismERP.Modules.Inventory.Domain.Enums;

namespace PrismERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class InventoryReservationRepository(PrismERPDbContext db) : IInventoryReservationRepository
{
    public Task<InventoryReservation?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default) =>
        db.InventoryReservations.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<decimal> GetActiveReservedQuantityAsync(
        int inventoryBalanceId,
        CancellationToken cancellationToken = default) =>
        db.InventoryReservations
            .Where(r => r.InventoryBalanceId == inventoryBalanceId && r.Status == InventoryReservationStatus.Active)
            .SumAsync(r => r.Quantity - r.FulfilledQuantity, cancellationToken);

    public Task<InventoryReservation?> GetActiveByReferenceAsync(
        InventoryReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default) =>
        db.InventoryReservations.FirstOrDefaultAsync(
            r => r.ReferenceType == referenceType
                && r.ReferenceId == referenceId
                && r.Status == InventoryReservationStatus.Active,
            cancellationToken);

    public async Task<List<InventoryReservation>?> GetActivesByReferencesAsync(
        InventoryReferenceType referenceType,
        HashSet<int> referenceIds,
        CancellationToken cancellationToken = default) => await

        db.InventoryReservations
        .Where(r => r.ReferenceType == referenceType 
                    && referenceIds.Contains(r.ReferenceId) 
                    && r.Status == InventoryReservationStatus.Active).ToListAsync(cancellationToken);

    public async Task<List<InventoryReservation>> GetByReferencesForUpdateAsync(
        InventoryReferenceType referenceType,
        HashSet<int> referenceIds,
        CancellationToken cancellationToken = default)
    {
        if (referenceIds.Count == 0)
            return [];

        return await db.InventoryReservations
            .Where(r => r.ReferenceType == referenceType
                && referenceIds.Contains(r.ReferenceId)
                && (r.Status == InventoryReservationStatus.Active
                    || r.Status == InventoryReservationStatus.Fulfilled))
            .ToListAsync(cancellationToken);
    }

    public Task<IReadOnlyCollection<InventoryReservation>> GetByBalanceIdAsync(
        int inventoryBalanceId,
        CancellationToken cancellationToken = default) =>
        db.InventoryReservations
            .AsNoTracking()
            .Where(r => r.InventoryBalanceId == inventoryBalanceId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyCollection<InventoryReservation>)t.Result, cancellationToken);

    public void Add(InventoryReservation reservation) => db.InventoryReservations.Add(reservation);

    public void Update(InventoryReservation reservation) => db.InventoryReservations.Update(reservation);
}
