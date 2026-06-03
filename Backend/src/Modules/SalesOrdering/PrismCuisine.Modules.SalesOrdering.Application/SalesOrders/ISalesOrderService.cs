namespace PrismCuisine.Modules.SalesOrdering.Application.SalesOrders;

public interface ISalesOrderService
{
    Task<IReadOnlyCollection<SalesOrderSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SalesOrderDto?> GetByIdAsync(int salesOrderId, CancellationToken cancellationToken = default);
    Task<SalesOrderDto> CreateAsync(CreateSalesOrderRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int salesOrderId, UpdateSalesOrderRequest request, CancellationToken cancellationToken = default);
    Task ApproveAsync(int SalesOrsalesOrderIdderId, CancellationToken cancellationToken = default);
    Task CancelAsync(int salesOrderId, CancellationToken cancellationToken = default);
}