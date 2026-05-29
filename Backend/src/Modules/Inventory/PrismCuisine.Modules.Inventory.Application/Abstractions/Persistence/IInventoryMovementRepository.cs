using PrismCuisine.Modules.Inventory.Domain.Entities;

namespace PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;

public interface IInventoryMovementRepository
{
    Task<IReadOnlyCollection<InventoryMovement>> GetByBalanceIdAsync(
        Guid inventoryBalanceId,
        CancellationToken cancellationToken = default);
    void Add(InventoryMovement movement);
}
