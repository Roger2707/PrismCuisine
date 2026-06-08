using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;
using PrismCuisine.Modules.Inventory.Domain.Entities;
using PrismCuisine.Modules.Inventory.Domain.Enums;

namespace PrismCuisine.Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class InventoryMovementRepository(PrismCuisineDbContext db) : IInventoryMovementRepository
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

    public void Add(InventoryMovement movement) => db.InventoryMovements.Add(movement);
}
