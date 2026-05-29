using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;
using PrismCuisine.Modules.Inventory.Domain.Entities;
using PrismCuisine.Modules.Inventory.Domain.Enums;

namespace PrismCuisine.Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class InventoryReservationRepository(PrismCuisineDbContext db) : IInventoryReservationRepository
{
    public Task<InventoryReservation?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.InventoryReservations.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<decimal> GetActiveReservedQuantityAsync(
        Guid inventoryBalanceId,
        CancellationToken cancellationToken = default) =>
        db.InventoryReservations
            .Where(r => r.InventoryBalanceId == inventoryBalanceId && r.Status == InventoryReservationStatus.Active)
            .SumAsync(r => r.Quantity - r.FulfilledQuantity, cancellationToken);

    public Task<InventoryReservation?> GetActiveByReferenceAsync(
        InventoryReferenceType referenceType,
        Guid referenceId,
        CancellationToken cancellationToken = default) =>
        db.InventoryReservations.FirstOrDefaultAsync(
            r => r.ReferenceType == referenceType
                && r.ReferenceId == referenceId
                && r.Status == InventoryReservationStatus.Active,
            cancellationToken);

    public void Add(InventoryReservation reservation) => db.InventoryReservations.Add(reservation);

    public void Update(InventoryReservation reservation) => db.InventoryReservations.Update(reservation);
}
