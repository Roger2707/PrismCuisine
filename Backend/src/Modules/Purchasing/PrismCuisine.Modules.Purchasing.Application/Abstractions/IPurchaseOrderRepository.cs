using PrismCuisine.Modules.Purchasing.Application.PurchaseOrders.Queries.GetPurchaseOrderById;
using PrismCuisine.Modules.Purchasing.Domain.Entities;

namespace PrismCuisine.Modules.Purchasing.Application.Abstractions;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByIdWithLinesForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PurchaseOrderDto?> GetByIdWithLinesAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(PurchaseOrder order);
    void Update(PurchaseOrder order);
}
