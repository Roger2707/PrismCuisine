using PrismERP.Modules.Inventory.Domain.Entities;
using PrismERP.Modules.Inventory.Domain.Enums;

namespace PrismERP.Modules.Inventory.Application.Abstractions.Persistence;

public interface IInventoryMovementRepository
{
    Task<IReadOnlyCollection<InventoryMovement>> GetByBalanceIdAsync(
        int inventoryBalanceId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryMovement>> GetIssuesByDeliveryReferenceAsync(
        InventoryReferenceType referenceType,
        string reference,
        IReadOnlyCollection<int> referenceIds,
        CancellationToken cancellationToken = default);
    void Add(InventoryMovement movement);
}
