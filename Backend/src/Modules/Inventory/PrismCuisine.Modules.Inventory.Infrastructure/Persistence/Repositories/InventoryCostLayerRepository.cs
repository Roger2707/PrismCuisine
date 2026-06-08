using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;
using PrismCuisine.Modules.Inventory.Domain.Entities;

namespace PrismCuisine.Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class InventoryCostLayerRepository(PrismCuisineDbContext db) : IInventoryCostLayerRepository
{
    public async Task<IReadOnlyCollection<InventoryCostLayer>> GetAvailableLayersForUpdateAsync(
        int inventoryBalanceId,
        CancellationToken cancellationToken = default) =>
        await db.InventoryCostLayers
            .Where(l => l.InventoryBalanceId == inventoryBalanceId && l.QuantityRemaining > 0)
            .OrderBy(l => l.ReceivedAt)
            .ThenBy(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<InventoryCostLayer>> GetAvailableLayersForUpdateByBalanceIdsAsync(
        IReadOnlyCollection<int> inventoryBalanceIds,
        CancellationToken cancellationToken = default)
    {
        if (inventoryBalanceIds.Count == 0)
            return [];

        return await db.InventoryCostLayers
            .Where(l => inventoryBalanceIds.Contains(l.InventoryBalanceId) && l.QuantityRemaining > 0)
            .OrderBy(l => l.InventoryBalanceId)
            .ThenBy(l => l.ReceivedAt)
            .ThenBy(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryCostLayer>> GetByIdsForUpdateAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return [];

        return await db.InventoryCostLayers
            .Where(l => ids.Contains(l.Id))
            .ToListAsync(cancellationToken);
    }

    public void Add(InventoryCostLayer layer) => db.InventoryCostLayers.Add(layer);

    public void Update(InventoryCostLayer layer) => db.InventoryCostLayers.Update(layer);
}
