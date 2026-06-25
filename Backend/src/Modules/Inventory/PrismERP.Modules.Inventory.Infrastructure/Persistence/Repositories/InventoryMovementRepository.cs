using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Domain.Entities;
using PrismERP.Modules.Inventory.Domain.Enums;

namespace PrismERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class InventoryMovementRepository(PrismERPDbContext db) : IInventoryMovementRepository
{
    public async Task<IReadOnlyCollection<InventoryMovement>> GetByBalanceIdAsync(
        int inventoryBalanceId,
        CancellationToken cancellationToken = default) =>
        await db.InventoryMovements
            .AsNoTracking()
            .Where(m => m.InventoryBalanceId == inventoryBalanceId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<InventoryMovement>> GetIssuesByDeliveryReferenceAsync(
        InventoryReferenceType referenceType,
        string reference,
        IReadOnlyCollection<int> referenceIds,
        CancellationToken cancellationToken = default)
    {
        if (referenceIds.Count == 0)
            return [];

        return await db.InventoryMovements
            .AsNoTracking()
            .Where(m => m.MovementType == InventoryMovementType.Issue
                && m.ReferenceType == referenceType
                && m.Reference == reference
                && m.ReferenceId != null
                && referenceIds.Contains(m.ReferenceId.Value))
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryMovement>> GetReceiptByPurchaseOrderReferenceAsync(
        InventoryReferenceType referenceType,
        string reference,
        IReadOnlyCollection<int> referenceIds,
        CancellationToken cancellationToken = default)
    {
        if (referenceIds.Count == 0)
            return [];

        return await db.InventoryMovements
            .AsNoTracking()
            .Where(m => m.MovementType == InventoryMovementType.Receipt
                && m.ReferenceType == referenceType
                && m.Reference == reference
                && m.ReferenceId != null
                && referenceIds.Contains(m.ReferenceId.Value))
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public void Add(InventoryMovement movement) => db.InventoryMovements.Add(movement);
}
