using PrismCuisine.Modules.SalesOrdering.Application.SalesOrders;
using PrismCuisine.Modules.SalesOrdering.Domain.Entities;

namespace PrismCuisine.Modules.SalesOrdering.Application.Abtractions;

public interface ISalesOrderRepository
{
    Task<SalesOrder?> GetByIdWithLinesForUpdateAsync(int id, CancellationToken cancellationToken = default);
    Task<SalesOrderDto?> GetByIdWithLinesAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SalesOrderSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<int> GetCountForDateAsync(DateTime date, CancellationToken cancellationToken = default);
    void Add(SalesOrder order);
    void Update(SalesOrder order);
}