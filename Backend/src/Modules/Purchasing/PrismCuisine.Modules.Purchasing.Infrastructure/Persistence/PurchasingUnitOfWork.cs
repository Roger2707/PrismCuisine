using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Purchasing.Application.Abstractions;
using PrismCuisine.Modules.Purchasing.Infrastructure.Persistence.Repositories;

namespace PrismCuisine.Modules.Purchasing.Infrastructure.Persistence;

internal sealed class PurchasingUnitOfWork(PrismCuisineDbContext db) : IPurchasingUnitOfWork
{
    public IPurchaseOrderRepository PurchaseOrders { get; } = new PurchaseOrderRepository(db);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
