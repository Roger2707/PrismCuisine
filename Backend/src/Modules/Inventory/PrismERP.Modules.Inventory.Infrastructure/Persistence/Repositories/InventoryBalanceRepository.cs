using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Domain.Entities;

namespace PrismERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class InventoryBalanceRepository(PrismERPDbContext db) : IInventoryBalanceRepository
{
    public Task<InventoryBalance?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.InventoryBalances.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<InventoryBalance?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default) =>
        db.InventoryBalances.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<InventoryBalance?> GetByIdForUpdateWithLockAsync(int id, CancellationToken cancellationToken = default) =>
        db.InventoryBalances
            .FromSqlInterpolated($"SELECT * FROM [inventory].InventoryBalances WITH (UPDLOCK, ROWLOCK) WHERE Id = {id}")
            .FirstOrDefaultAsync(cancellationToken);

    public async Task PermisticLockingByBalanceIdsAsync(HashSet<int> balanceIds, CancellationToken cancellationToken = default)
    {
        if (balanceIds == null || !balanceIds.Any()) return;
        var idsString = string.Join(",", balanceIds);
        var sql = $@"
                    SELECT 1 FROM [inventory].InventoryBalances WITH (ROWLOCK, UPDLOCK) 
                    WHERE Id IN ({idsString})
                    ORDER BY Id
                    ";
        await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryBalance>> GetByIdsForUpdateAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return [];

        return await db.InventoryBalances
            .Where(b => ids.Contains(b.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<InventoryBalance?> GetByProductAndWarehouseAsync(
        int productId,
        int warehouseId,
        CancellationToken cancellationToken = default) =>
        db.InventoryBalances.FirstOrDefaultAsync(
            b => b.ProductId == productId && b.WarehouseId == warehouseId,
            cancellationToken);

    public async Task<List<InventoryBalance>> GetByGroupProductAndWarehouseAsync(
        List<(int ProductId, int WarehouseId)> keys, CancellationToken cancellationToken = default)
    {
        if (keys.Count == 0)
            return [];

        var productIds = keys.Select(k => k.ProductId).Distinct().ToList();
        var warehouseIds = keys.Select(k => k.WarehouseId).Distinct().ToList();
        var keySet = keys.ToHashSet();

        var candidates = await db.InventoryBalances
            .Where(b => productIds.Contains(b.ProductId) && warehouseIds.Contains(b.WarehouseId))
            .ToListAsync(cancellationToken);

        return candidates
            .Where(b => keySet.Contains((b.ProductId, b.WarehouseId)))
            .ToList();
    }

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
