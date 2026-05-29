using PrismCuisine.Modules.Inventory.Domain.Entities;
using PrismCuisine.Modules.Inventory.Domain.Enums;

namespace PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;

public interface IInventoryReservationRepository
{
    Task<InventoryReservation?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<decimal> GetActiveReservedQuantityAsync(
        Guid inventoryBalanceId,
        CancellationToken cancellationToken = default);
    Task<InventoryReservation?> GetActiveByReferenceAsync(
        InventoryReferenceType referenceType,
        Guid referenceId,
        CancellationToken cancellationToken = default);
    void Add(InventoryReservation reservation);
    void Update(InventoryReservation reservation);
}
