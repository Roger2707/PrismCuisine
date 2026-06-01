using PrismCuisine.Modules.Purchasing.Application.PurchaseOrders;
using PrismCuisine.Modules.Purchasing.Domain.Entities;

namespace PrismCuisine.Modules.Purchasing.Application.Abstractions;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByIdWithLinesForUpdateAsync(int id, CancellationToken cancellationToken = default);
    Task<PurchaseOrderDto?> GetByIdWithLinesAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PurchaseOrderSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<int> GetCountForDateAsync(DateTime date, CancellationToken cancellationToken = default);
    void Add(PurchaseOrder order);
    void Update(PurchaseOrder order);
}
