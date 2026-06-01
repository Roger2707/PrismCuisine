using PrismCuisine.Modules.Inventory.Domain.Entities;

namespace PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;

public interface IInventoryCostLayerRepository
{
    Task<IReadOnlyCollection<InventoryCostLayer>> GetAvailableLayersForUpdateAsync(
        int inventoryBalanceId,
        CancellationToken cancellationToken = default);
    void Add(InventoryCostLayer layer);
    void Update(InventoryCostLayer layer);
}
