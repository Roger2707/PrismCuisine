using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.SalesOrdering.Application.Abtractions;
using PrismERP.Modules.SalesOrdering.Domain.Entities;
using PrismERP.Modules.SalesOrdering.Domain.Enums;
using PrismERP.Modules.Inventory.Application.Inventory;
using PrismERP.Modules.Inventory.Application.Inventory.Workflows;

namespace PrismERP.Modules.SalesOrdering.Application.SalesOrders;
public sealed class SalesOrderService(
    ISalesOrderingUnitOfWork unitOfWork
    , IInventorySalesReservationWorkflowService inventoryReservations) : ISalesOrderService
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
            throw new ValidationException("customerId", "CustomerId is required.");

        if (!await unitOfWork.Customers.IsExists(request.CustomerId))
            throw new ValidationException("customerId", $"Customer with id '{request.CustomerId}' does not exist.");

        if (request.Lines is null || request.Lines.Count == 0)
            throw new BusinessException("Sales order must have at least one line.");

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
            ?? throw new NotFoundException($"Sales order with id '{salesOrderId}' was not found.");

        if(request.CustomerId == 0 || string.IsNullOrWhiteSpace(request.CustomerId.ToString()))
            throw new ValidationException("customerId", "CustomerId is required.");

        if (request.Lines is null || request.Lines.Count == 0)
            throw new BusinessException("Sales order must have at least one line.");

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
        await unitOfWork.ExecuteInTransactionWithRetryAsync(async ct =>
        {
            // Reload inside the transaction so every retry sees the latest committed state.
            var salesOrder = await unitOfWork.SalesOrders.GetByIdWithLinesForUpdateAsync(salesOrderId, ct)
                ?? throw new NotFoundException($"Sales order with id '{salesOrderId}' was not found.");

            // Guard: if another user already approved this SO, surface 409 immediately — no retry.
            if (salesOrder.Status != SalesOrderStatus.Draft)
                throw new ConflictException(
                    $"Sales order '{salesOrder.OrderNumber}' is already '{salesOrder.Status}'. Refresh and try again.");

            const int warehouseId = 1; // TODO: assign warehouse based on product or sales order

            // UPDLOCK acquired inside ReserveForSalesOrderAsync serialises concurrent reserves
            // for the same product/warehouse — prevents oversell without locking the whole table.
            await inventoryReservations.ReserveForSalesOrderAsync(
                new CreateReservationRequest(
                    salesOrder.Lines
                        .Select(l => new CreateReservationLine(
                            l.ProductId,
                            warehouseId,
                            l.QuantityOrdered,
                            l.Id,
                            $"Reservation for sales order {salesOrder.OrderNumber}, line {l.Id}"))
                        .ToList()),
                ct);

            salesOrder.Approve();
            unitOfWork.SalesOrders.Update(salesOrder);
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);
    }

    public async Task CancelAsync(int salesOrderId, CancellationToken cancellationToken = default)
    {
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var salesOrder = await unitOfWork.SalesOrders.GetByIdWithLinesForUpdateAsync(salesOrderId, ct)
                ?? throw new NotFoundException($"Sales order with id '{salesOrderId}' was not found.");

            if(salesOrder.Status == SalesOrderStatus.Confirmed)
            {
                // 1. Handle DeliveryNotes have been created
                var deliveryNotes = await unitOfWork.DeliveryNotes.GetBySalesOrderIdAsync(salesOrderId, ct);
                if(deliveryNotes != null && deliveryNotes.Count > 0)
                {
                    if (deliveryNotes.Any(d => d.Status == DeliveryNoteStatus.Posted))
                        throw new BusinessException($"Posted DeliveryNote cannot be Cancel for this SalesOrder ! Please use Refund instead of it!");

                    // delete all Deliverynote in DB if Draft status
                    unitOfWork.DeliveryNotes.RemoveRange(deliveryNotes);
                }

                // 2. Check SO Line DeliveriedQty
                if (salesOrder.Lines.Any(l => l.QuantityDelivered > 0))
                    throw new BusinessException($"There are at least one SO Line has DelieveriedQty !");

                // 3. Handle Reservations, if 1 DN created from SO and Posted, SO status surely changed to partial or full deliveried
                // we block this case first so let's believe qty cannot be exported from CostLayer. Change status is enough
                var referenceIds = salesOrder.Lines.Select(l => l.Id).ToHashSet();
                await inventoryReservations.ReleaseReservationsAsync(referenceIds, ct);
            }

            salesOrder.Cancel();
            unitOfWork.SalesOrders.Update(salesOrder);
            await unitOfWork.SaveChangesAsync(ct);

        }, cancellationToken);
    }

    #endregion

    #region Helper Methods

    private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var count = await unitOfWork.SalesOrders.GetCountForDateAsync(today, cancellationToken);
        return $"SO-{today:yyyyMMdd}-{(count + 1):D4}";
    }

    #endregion
}
