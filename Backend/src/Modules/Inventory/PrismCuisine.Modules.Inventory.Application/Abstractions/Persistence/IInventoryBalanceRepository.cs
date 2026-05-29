using PrismCuisine.Modules.Inventory.Domain.Entities;

namespace PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;

public interface IInventoryBalanceRepository
{
    Task<InventoryBalance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InventoryBalance?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InventoryBalance?> GetByProductAndWarehouseAsync(
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryBalance>> GetLowStockAsync(CancellationToken cancellationToken = default);
    void Add(InventoryBalance balance);
    void Update(InventoryBalance balance);
}
