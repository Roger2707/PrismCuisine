using PrismERP.BuildingBlocks.Application.Abstractions.Persistence;

namespace PrismERP.Modules.SalesOrdering.Application.Abtractions;
public interface ISalesOrderingUnitOfWork : IUnitOfWork
{
    ICustomerRepository Customers { get; }
    ISalesOrderRepository SalesOrders { get; }
    IDeliveryNoteRepository DeliveryNotes { get; }

    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Executes <paramref name="action"/> in a transaction, retrying up to 3 times on
    /// <see cref="Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException"/>.
    /// The change tracker is cleared before every attempt so that entity reloads inside
    /// the action always hit the database instead of EF's identity cache.
    /// Non-retriable exceptions (e.g. <see cref="PrismERP.BuildingBlocks.Domain.Exceptions.ConflictException"/>,
    /// <see cref="PrismERP.BuildingBlocks.Domain.Exceptions.BusinessException"/>) propagate immediately.
    /// </summary>
    Task ExecuteInTransactionWithRetryAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default
    );
}
