using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;
using PrismCuisine.Modules.Inventory.Domain.Entities;

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

    public void Add(InventoryMovement movement) => db.InventoryMovements.Add(movement);
}
