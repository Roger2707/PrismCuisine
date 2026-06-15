using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Finance.Application.Invoices;
using PrismERP.Modules.Finance.Domain.Entities;
using PrismERP.Modules.Finance.Domain.Enums;
using PrismERP.Modules.Inventory.Application.Inventory;
using PrismERP.Modules.Inventory.Domain.Enums;
using PrismERP.Modules.SalesOrdering.Application.Abtractions;
using PrismERP.Modules.SalesOrdering.Domain.Entities;
using PrismERP.Modules.SalesOrdering.Domain.Enums;

namespace PrismERP.Modules.SalesOrdering.Application.Deliveries;

public sealed class DeliveryNoteService(
    ISalesOrderingUnitOfWork unitOfWork
    , IInventoryPostingService inventoryPostingService
    , IInvoiceService invoiceService
    ) : IDeliveryNoteService
{
    #region Read

    public async Task<IReadOnlyCollection<DeliveryNoteSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var deliveries = await unitOfWork.DeliveryNotes.GetAllAsync(cancellationToken);
        return deliveries;
    }

    public async Task<DeliveryNoteDto?> GetByIdAsync(int deliveryNoteId, CancellationToken cancellationToken = default)
    {
        var delivery = await unitOfWork.DeliveryNotes.GetByIdWithLinesAsync(deliveryNoteId, cancellationToken);
        return delivery;
    }

    #endregion

    #region Write

    public async Task<DeliveryNoteDto> CreateAsync(CreateDeliveryNoteRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Lines is null || request.Lines.Count == 0)
            throw new BusinessException("Delivery note must have at least one line.");

        var salesOrder = await unitOfWork.SalesOrders.GetByIdWithLinesForUpdateAsync(request.SalesOrderId, cancellationToken)
            ?? throw new ValidationException("salesOrderId", $"Sales order with id '{request.SalesOrderId}' does not exist.");

        if (salesOrder.Status is not SalesOrderStatus.Confirmed and not SalesOrderStatus.PartialDelivery)
            throw new BusinessException("Delivery note can only be created for confirmed or partially delivered sales orders.");

        var deliveryNumber = await GenerateDeliveryNumberAsync(cancellationToken);
        var deliveryNote = DeliveryNote.CreateDraft(
            deliveryNumber,
            salesOrder.Id,
            salesOrder.CustomerId,
            salesOrder.CustomerName,
            salesOrder.OrderNumber,
            salesOrder.Status,
            request.Notes);

        foreach (var line in request.Lines)
        {
            var orderLine = salesOrder.Lines.FirstOrDefault(l => l.Id == line.SalesOrderLineId)
                ?? throw new BusinessException(
                    $"Sales order line with id '{line.SalesOrderLineId}' does not exist in sales order '{request.SalesOrderId}'.");

            deliveryNote.AddLine(line.QuantityDelivered, orderLine);
        }

        unitOfWork.DeliveryNotes.Add(deliveryNote);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return (await unitOfWork.DeliveryNotes.GetByIdWithLinesAsync(deliveryNote.Id, cancellationToken))!;
    }

    public async Task UpdateAsync(int deliveryNoteId, UpdateDeliveryNoteRequest request, CancellationToken cancellationToken = default)
    {
        var deliveryNote = await unitOfWork.DeliveryNotes.GetByIdWithLinesForUpdateAsync(deliveryNoteId, cancellationToken)
            ?? throw new NotFoundException($"Delivery note with id '{deliveryNoteId}' does not exist.");

        if (request.Lines is null || request.Lines.Count == 0)
            throw new BusinessException("Delivery note must have at least one line.");

        var salesOrder = await unitOfWork.SalesOrders.GetByIdWithLinesForUpdateAsync(
            deliveryNote.SalesOrderId,
            cancellationToken)
            ?? throw new NotFoundException($"Sales order with id '{deliveryNote.SalesOrderId}' does not exist.");

        if (salesOrder.Status is not SalesOrderStatus.Confirmed and not SalesOrderStatus.PartialDelivery)
            throw new BusinessException("Delivery note can only be updated for confirmed or partially delivered sales orders.");

        var preparedLines = request.Lines.Select(line =>
        {
            var orderLine = salesOrder.Lines.FirstOrDefault(l => l.Id == line.SalesOrderLineId)
                ?? throw new BusinessException(
                    $"Sales order line with id '{line.SalesOrderLineId}' does not exist in sales order '{deliveryNote.SalesOrderId}'.");

            return (line.QuantityDelivered, orderLine);
        }).ToList();

        deliveryNote.UpdateDraft(request.Notes);
        deliveryNote.ReplaceLines(preparedLines);
        unitOfWork.DeliveryNotes.Update(deliveryNote);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Business Operations

    public async Task PostAsync(int deliveryNoteId, CancellationToken cancellationToken = default)
    {
        var deliveryNote = await unitOfWork.DeliveryNotes.GetByIdWithLinesForUpdateAsync(deliveryNoteId, cancellationToken)
            ?? throw new NotFoundException($"Delivery note '{deliveryNoteId}' was not found.");

        var salesOrder = await unitOfWork.SalesOrders.GetByIdWithLinesForUpdateAsync(
            deliveryNote.SalesOrderId,
            cancellationToken)
            ?? throw new NotFoundException($"Sales order '{deliveryNote.SalesOrderId}' was not found.");

        if (deliveryNote.Lines.Count == 0)
            throw new BusinessException("Delivery note must have at least one line.");

        // because reservation hold referenceId is SalesOrderLineId
        var deliveryLineIds = deliveryNote.Lines.Select(l => l.SalesOrderLineId).ToHashSet();
        var reservations = await inventoryPostingService.GetActivesByReferencesAsync(
            InventoryReferenceType.SalesOrder,
            deliveryLineIds,
            cancellationToken)
            ?? [];

        var salesOrderMap = salesOrder.Lines.ToDictionary(s => s.Id);

        var reservationByLineId = reservations.ToDictionary(r => r.ReferenceId);
        var fulfillLines = new List<FulfillReservationLine>(deliveryNote.Lines.Count);
        var invocieLineRequests = new List<CreateInvoiceLineRequest>(deliveryNote.Lines.Count);

        foreach (var line in deliveryNote.Lines)
        {
            if (!reservationByLineId.TryGetValue(line.SalesOrderLineId, out var reservation))
            {
                throw new BusinessException(
                    $"No active inventory reservation found for sales order line '{line.SalesOrderLineId}'.");
            }

            if (line.QuantityDelivered > reservation.RemainingQuantity)
            {
                throw new BusinessException(
                    $"Delivery quantity '{line.QuantityDelivered}' exceeds remaining reservation '{reservation.RemainingQuantity}' for sales order line '{line.SalesOrderLineId}'.");
            }

            // 1. Export
            fulfillLines.Add(new FulfillReservationLine(
                reservation,
                line.QuantityDelivered,
                deliveryNote.DeliveryNumber,
                $"Delivery note {deliveryNote.DeliveryNumber}, SO line {line.SalesOrderLineId}"));

            // 2. Create Invoice Line Request (AR flow)
            var invocieLineRequest = new CreateInvoiceLineRequest(
                line.ProductId.ToString(), line.ProductName, "", line.QuantityDelivered
                , salesOrderMap[line.SalesOrderLineId].UnitPrice
                , salesOrderMap[line.SalesOrderLineId].VATRate
                , salesOrderMap[line.SalesOrderLineId].DiscountPercent);
            invocieLineRequests.Add(invocieLineRequest);
        }
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await inventoryPostingService.FulfillReservationsAsync(fulfillLines, ct);
            deliveryNote.Post(salesOrder);
            unitOfWork.DeliveryNotes.Update(deliveryNote);
            unitOfWork.SalesOrders.Update(salesOrder);

            // Create Invoice (AR) for Accounting
            var invoiceNumber = await invoiceService.GenerateInvoiceNumberAsync(cancellationToken);
            var invoiceDto = await invoiceService.CreateAsync(
                new CreateInvoiceRequest(
                    invoiceNumber, InvoiceType.SalesInvoice, DateTime.UtcNow, null, deliveryNote.CustomerName, "",
                    salesOrder.Id, deliveryNoteId, null, null, ""
                    , invocieLineRequests), cancellationToken);

            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);
    }

    public async Task CancelAsync(int deliveryNoteId, CancellationToken cancellationToken = default)
    {
        var deliveryNote = await unitOfWork.DeliveryNotes.GetByIdWithLinesForUpdateAsync(deliveryNoteId, cancellationToken)
            ?? throw new NotFoundException($"Delivery note '{deliveryNoteId}' was not found.");

        var salesOrder = await unitOfWork.SalesOrders.GetByIdWithLinesForUpdateAsync(
            deliveryNote.SalesOrderId,
            cancellationToken)
            ?? throw new NotFoundException($"Sales order '{deliveryNote.SalesOrderId}' was not found.");

        var returnLines = deliveryNote.Lines
            .Select(l => new ReturnDeliveryLine(l.SalesOrderLineId, l.QuantityDelivered))
            .ToList();

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await inventoryPostingService.ReturnDeliveryIssuesAsync(
                deliveryNote.DeliveryNumber,
                returnLines,
                ct);

            deliveryNote.Cancel(salesOrder);
            unitOfWork.DeliveryNotes.Update(deliveryNote);
            unitOfWork.SalesOrders.Update(salesOrder);
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);
    }

    #endregion

    #region Helper Methods

    private async Task<string> GenerateDeliveryNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var count = await unitOfWork.DeliveryNotes.GetCountForDateAsync(today, cancellationToken);
        return $"DN-{today:yyyyMMdd}-{(count + 1):D4}";
    }

    #endregion
}
