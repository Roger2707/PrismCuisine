using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;
using PrismCuisine.Modules.Inventory.Domain.Entities;
using PrismCuisine.Modules.Inventory.Domain.Enums;

namespace PrismCuisine.Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class InventoryReservationRepository(PrismCuisineDbContext db) : IInventoryReservationRepository
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

    public void Add(InventoryReservation reservation) => db.InventoryReservations.Add(reservation);

    public void Update(InventoryReservation reservation) => db.InventoryReservations.Update(reservation);
}
