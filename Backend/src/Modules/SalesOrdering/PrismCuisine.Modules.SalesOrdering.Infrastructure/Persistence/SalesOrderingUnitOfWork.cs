using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.SalesOrdering.Application.Abtractions;
using PrismCuisine.Modules.SalesOrdering.Infrastructure.Persistence.Repositories;

namespace PrismCuisine.Modules.SalesOrdering.Infrastructure.Persistence;

public sealed class SalesOrderingUnitOfWork(PrismCuisineDbContext db) : ISalesOrderingUnitOfWork
{
    public ICustomerRepository Customers { get; } = new CustomerRepository(db);

    public ISalesOrderRepository SalesOrders { get; } = new SalesOrderRepository(db);

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
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

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
