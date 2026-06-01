using PrismCuisine.Modules.Inventory.Domain.Entities;

namespace PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;

public interface IInventoryBalanceRepository
{
    Task<InventoryBalance?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<InventoryBalance?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);
    Task<InventoryBalance?> GetByProductAndWarehouseAsync(
        int productId,
        int warehouseId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryBalance>> GetLowStockAsync(CancellationToken cancellationToken = default);
    void Add(InventoryBalance balance);
    void Update(InventoryBalance balance);
}
