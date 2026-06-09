using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Purchasing.Application.Abstractions;
using PrismERP.Modules.Purchasing.Infrastructure.Persistence.Repositories;

namespace PrismERP.Modules.Purchasing.Infrastructure.Persistence;

internal sealed class PurchasingUnitOfWork(PrismERPDbContext db) : IPurchasingUnitOfWork
{
    public ISupplierRepository Suppliers { get; } = new SupplierRepository(db);
    public IPurchaseOrderRepository PurchaseOrders { get; } = new PurchaseOrderRepository(db);
    public IGoodsReceiptRepository GoodsReceipts { get; } = new GoodsReceiptRepository(db);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await action(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
