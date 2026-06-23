using LinqKit;
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

    public Task<List<InventoryBalance>> GetByGroupProductAndWarehouseAsync(
        List<(int ProductId, int WarehouseId)> keys, CancellationToken cancellationToken = default)
    {
        var predicate = PredicateBuilder.New<InventoryBalance>(false);

        foreach (var key in keys)
            predicate = predicate.Or(b => b.ProductId == key.ProductId && b.WarehouseId == key.WarehouseId);

        return db.InventoryBalances
            .AsExpandable()
            .Where(predicate)
            .ToListAsync(cancellationToken);
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
