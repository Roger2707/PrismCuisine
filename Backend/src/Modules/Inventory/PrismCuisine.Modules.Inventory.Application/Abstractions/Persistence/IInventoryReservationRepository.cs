using PrismCuisine.Modules.Inventory.Domain.Entities;
using PrismCuisine.Modules.Inventory.Domain.Enums;

namespace PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;

public interface IInventoryReservationRepository
{
    Task<InventoryReservation?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);
    Task<decimal> GetActiveReservedQuantityAsync(
        int inventoryBalanceId,
        CancellationToken cancellationToken = default);
    Task<InventoryReservation?> GetActiveByReferenceAsync(
        InventoryReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default);

    Task<List<InventoryReservation>?> GetActivesByReferencesAsync(
        InventoryReferenceType referenceType,
        HashSet<int> referenceIds,
        CancellationToken cancellationToken = default);
    Task<List<InventoryReservation>> GetByReferencesForUpdateAsync(
        InventoryReferenceType referenceType,
        HashSet<int> referenceIds,
        CancellationToken cancellationToken = default);

    void Add(InventoryReservation reservation);
    void Update(InventoryReservation reservation);
}
