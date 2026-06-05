using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.Inventory.Application.Inventory;
using PrismCuisine.Modules.Inventory.Domain.Enums;
using PrismCuisine.Modules.SalesOrdering.Application.Abtractions;
using PrismCuisine.Modules.SalesOrdering.Domain.Entities;
using PrismCuisine.Modules.SalesOrdering.Domain.Enums;

namespace PrismCuisine.Modules.SalesOrdering.Application.Deliveries;

internal sealed class DeliveryNoteService(ISalesOrderingUnitOfWork unitOfWork, IInventoryPostingService inventoryPostingService) : IDeliveryNoteService
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
        var salesOrder = await unitOfWork.SalesOrders.GetByIdWithLinesForUpdateAsync(request.SalesOrderId, cancellationToken);
        if (salesOrder is null)
            throw new ArgumentException($"Sales order with id '{request.SalesOrderId}' does not exist.");

        var deliveryNumber = await GenerateOrderNumberAsync(cancellationToken);
        var deliveryNote = DeliveryNote.CreateDraft(
            deliveryNumber
            , request.SalesOrderId, salesOrder.CustomerId, salesOrder.CustomerName, request.OrderNumber, salesOrder.Status
            , request.Notes);

        foreach (var line in request.Lines)
        {
            var orderLine = salesOrder.Lines.FirstOrDefault(l => l.Id == line.SalesOrderLineId)
                ?? throw new DomainException(
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
            ?? throw new DomainException($"Delivery note with id '{deliveryNoteId}' does not exist.");

        if (request.Lines is null || request.Lines.Count == 0)
            throw new DomainException("Delivery note must have at least one line.");

        var salesOrder = await unitOfWork.SalesOrders.GetByIdWithLinesForUpdateAsync(
            deliveryNote.SalesOrderId,
            cancellationToken)
            ?? throw new DomainException($"Sales order with id '{deliveryNote.SalesOrderId}' does not exist.");

        if (salesOrder.Status is not SalesOrderStatus.Confirmed and not SalesOrderStatus.PartialDelivery)
            throw new DomainException("Delivery note can only be updated for confirmed or partially delivered sales orders.");

        var preparedLines = request.Lines.Select(line =>
        {
            var orderLine = salesOrder.Lines.FirstOrDefault(l => l.Id == line.SalesOrderLineId)
                ?? throw new DomainException(
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
            ?? throw new DomainException($"Delivery note '{deliveryNoteId}' was not found.");

        var salesOrder = await unitOfWork.SalesOrders.GetByIdWithLinesForUpdateAsync(
            deliveryNote.SalesOrderId,
            cancellationToken)
            ?? throw new DomainException($"Sales order '{deliveryNote.SalesOrderId}' was not found.");

        if (deliveryNote.Lines.Count == 0)
            throw new DomainException("Delivery note must have at least one line.");

        var deliveryLineIds = deliveryNote.Lines.Select(l => l.SalesOrderLineId).ToHashSet();
        var reservations = await inventoryPostingService.GetActivesByReferencesAsync(
            InventoryReferenceType.SalesOrder,
            deliveryLineIds,
            cancellationToken)
            ?? [];

        var reservationByLineId = reservations.ToDictionary(r => r.ReferenceId);
        var fulfillLines = new List<FulfillReservationLine>(deliveryNote.Lines.Count);

        foreach (var line in deliveryNote.Lines)
        {
            if (!reservationByLineId.TryGetValue(line.SalesOrderLineId, out var reservation))
            {
                throw new DomainException(
                    $"No active inventory reservation found for sales order line '{line.SalesOrderLineId}'.");
            }

            if (line.QuantityDelivered > reservation.RemainingQuantity)
            {
                throw new DomainException(
                    $"Delivery quantity '{line.QuantityDelivered}' exceeds remaining reservation '{reservation.RemainingQuantity}' for sales order line '{line.SalesOrderLineId}'.");
            }

            fulfillLines.Add(new FulfillReservationLine(
                reservation,
                line.QuantityDelivered,
                deliveryNote.DeliveryNumber,
                $"Delivery note {deliveryNote.DeliveryNumber}, SO line {line.SalesOrderLineId}"));
        }

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await inventoryPostingService.FulfillReservationsAsync(fulfillLines, ct);

            deliveryNote.Post(salesOrder);
            unitOfWork.DeliveryNotes.Update(deliveryNote);
            unitOfWork.SalesOrders.Update(salesOrder);
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);
    }

    public async Task CancelAsync(int deliveryNoteId, CancellationToken cancellationToken = default)
    {
        var salesOrder = await unitOfWork.SalesOrders.GetByIdWithLinesForUpdateAsync(deliveryNoteId, cancellationToken)
            ?? throw new DomainException($"Sales order with id '{deliveryNoteId}' does not exist.");

        var deliveryNote = await unitOfWork.DeliveryNotes.GetByIdWithLinesForUpdateAsync(deliveryNoteId, cancellationToken)
            ?? throw new DomainException($"Delivery note with id '{deliveryNoteId}' does not exist.");
    }

    #endregion

    #region Helper Methods

    private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var count = await unitOfWork.DeliveryNotes.GetCountForDateAsync(today, cancellationToken);
        return $"PO-{today:yyyyMMdd}-{(count + 1):D4}";
    }

    #endregion
}
