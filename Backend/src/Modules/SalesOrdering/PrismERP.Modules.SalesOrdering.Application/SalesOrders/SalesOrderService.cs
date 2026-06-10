using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.SalesOrdering.Application.Abtractions;
using PrismERP.Modules.SalesOrdering.Domain.Entities;
using PrismERP.Modules.Inventory.Application.Inventory;

namespace PrismERP.Modules.SalesOrdering.Application.SalesOrders;
public sealed class SalesOrderService(
    ISalesOrderingUnitOfWork unitOfWork
    , IInventoryPostingService inventoryPosting) : ISalesOrderService
{

    #region Read

    public async Task<IReadOnlyCollection<SalesOrderSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await unitOfWork.SalesOrders.GetAllAsync(cancellationToken);
        return orders;
    }

    public async Task<SalesOrderDto?> GetByIdAsync(int salesOrderId, CancellationToken cancellationToken = default)
    {
        var order = await unitOfWork.SalesOrders.GetByIdWithLinesAsync(salesOrderId, cancellationToken);
        return order;
    }

    #endregion

    #region Write

    public async Task<SalesOrderDto> CreateAsync(CreateSalesOrderRequest request, CancellationToken cancellationToken = default)
    {
        #region Validations

        if (string.IsNullOrWhiteSpace(request.CustomerId.ToString()))
            throw new ArgumentException("CustomerId is required.");

        if (!await unitOfWork.Customers.IsExists(request.CustomerId))
            throw new ArgumentException($"Customer with id '{request.CustomerId}' does not exist.");

        if (request.Lines is null || request.Lines.Count == 0)
            throw new DomainException("Sales order must have at least one line.");

        string customerName = request?.CustomerName ?? "";
        if (!string.IsNullOrWhiteSpace(customerName))
        {
            var customer = await unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);
            customerName = customer?.Name ?? "";
        }

        #endregion

        var orderNumber = await GenerateOrderNumberAsync(cancellationToken);
        var salesOrder = SalesOrder.CreateDraft(orderNumber, request.CustomerId, customerName, request.Notes);

        foreach (var line in request.Lines)
        {
            salesOrder.AddLine(
                line.ProductId, line.ProductName, line.QuantityOrdered
                , line.UnitPrice, line.DiscountPercent, line.VATRate
            );
        }

        salesOrder.RecalculateTotals();

        unitOfWork.SalesOrders.Add(salesOrder);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return (await unitOfWork.SalesOrders.GetByIdWithLinesAsync(salesOrder.Id, cancellationToken))!;
    }

    public async Task UpdateAsync(int salesOrderId, UpdateSalesOrderRequest request, CancellationToken cancellationToken = default)
    {
        var salesOrder = await unitOfWork.SalesOrders.GetByIdWithLinesForUpdateAsync(salesOrderId, cancellationToken)
            ?? throw new DomainException($"Sales order with id '{salesOrderId}' was not found.");

        if(request.CustomerId == 0 || string.IsNullOrWhiteSpace(request.CustomerId.ToString()))
            throw new ArgumentException("CustomerId is required.");

        if (request.Lines is null || request.Lines.Count == 0)
            throw new DomainException("Sales order must have at least one line.");

        salesOrder.UpdateDraft(request.CustomerId, request.CustomerName, request.Notes);

        salesOrder.ReplaceLines(request.Lines
                .Select(l => (l.ProductId, l.ProductName, l.QuantityOrdered, l.UnitPrice, l.DiscountPercent, l.VATRate))
                .ToList());

        salesOrder.RecalculateTotals();
        unitOfWork.SalesOrders.Update(salesOrder);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Business Actions

    public async Task ApproveAsync(int salesOrderId, CancellationToken cancellationToken = default)
    {
        var salesOrder = await unitOfWork.SalesOrders.GetByIdWithLinesForUpdateAsync(salesOrderId, cancellationToken)
            ?? throw new DomainException($"Sales order with id '{salesOrderId}' was not found.");

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            const int warehouseId = 1; // TODO: assign warehouse based on product or sales order

            await inventoryPosting.ReserveAsync(
                        new CreateReservationRequest(
                            salesOrder.Lines
                            .Select(l => new CreateReservationLine(
                                l.ProductId,
                                warehouseId,
                                l.QuantityOrdered,
                                l.Id,
                                $"Reservation for sales order {salesOrder.OrderNumber}, line {l.Id}"))
                            .ToList()), ct);

            salesOrder.Approve();
            unitOfWork.SalesOrders.Update(salesOrder);
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);
    }

    public async Task CancelAsync(int salesOrderId, CancellationToken cancellationToken = default)
    {
        var salesOrder = await unitOfWork.SalesOrders.GetByIdWithLinesForUpdateAsync(salesOrderId, cancellationToken)
            ?? throw new DomainException($"Sales order with id '{salesOrderId}' was not found.");

        salesOrder.Cancel();
        unitOfWork.SalesOrders.Update(salesOrder);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Helper Methods

    private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var count = await unitOfWork.SalesOrders.GetCountForDateAsync(today, cancellationToken);
        return $"PO-{today:yyyyMMdd}-{(count + 1):D4}";
    }

    #endregion
}
