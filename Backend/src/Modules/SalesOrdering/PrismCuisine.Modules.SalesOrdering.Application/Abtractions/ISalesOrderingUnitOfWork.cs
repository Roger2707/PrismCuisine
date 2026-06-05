using PrismCuisine.BuildingBlocks.Application.Abstractions.Persistence;

namespace PrismCuisine.Modules.SalesOrdering.Application.Abtractions;
public interface ISalesOrderingUnitOfWork : IUnitOfWork
{
    ICustomerRepository Customers { get; }
    ISalesOrderRepository SalesOrders { get; }
    IDeliveryNoteRepository DeliveryNotes { get; }

    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default
    );
}
