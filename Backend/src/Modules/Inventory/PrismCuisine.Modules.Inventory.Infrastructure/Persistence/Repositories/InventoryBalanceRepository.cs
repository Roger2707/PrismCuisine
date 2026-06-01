using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;
using PrismCuisine.Modules.Inventory.Domain.Entities;

namespace PrismCuisine.Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class InventoryBalanceRepository(PrismCuisineDbContext db) : IInventoryBalanceRepository
{
    public Task<InventoryBalance?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.InventoryBalances.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<InventoryBalance?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default) =>
        db.InventoryBalances.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<InventoryBalance?> GetByProductAndWarehouseAsync(
        int productId,
        int warehouseId,
        CancellationToken cancellationToken = default) =>
        db.InventoryBalances.FirstOrDefaultAsync(
            b => b.ProductId == productId && b.WarehouseId == warehouseId,
            cancellationToken);

    public async Task<IReadOnlyCollection<InventoryBalance>> GetLowStockAsync(
        CancellationToken cancellationToken = default) =>
        await db.InventoryBalances
            .AsNoTracking()
            .Where(b => b.QuantityOnHand <= b.ReorderLevel)
            .OrderBy(b => b.QuantityOnHand)
            .ToListAsync(cancellationToken);

    public void Add(InventoryBalance balance) => db.InventoryBalances.Add(balance);

    public void Update(InventoryBalance balance) => db.InventoryBalances.Update(balance);
}
