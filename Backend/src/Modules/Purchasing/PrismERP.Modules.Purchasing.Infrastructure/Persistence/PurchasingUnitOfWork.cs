using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Domain.Exceptions;
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

    public async Task ExecuteInTransactionWithRetryAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        const int maxRetries = 3;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            // Clear the identity cache so every attempt reads fresh rows from the database.
            db.ChangeTracker.Clear();

            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await action(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return;
            }
            catch (DbUpdateConcurrencyException) when (attempt < maxRetries)
            {
                await transaction.RollbackAsync(cancellationToken);
                // continue — next iteration will clear the tracker and reload from DB
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        // All retries exhausted — surface as a 409 Conflict so the client can refresh and retry.
        throw new ConflictException("Data was modified by another user. Please refresh and try again.");
    }
}
