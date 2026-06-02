using PrismCuisine.BuildingBlocks.Application.Abstractions.Messaging;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.Purchasing.Application.Abstractions;
using PrismCuisine.Modules.Purchasing.Domain.Entities;
using PrismCuisine.Modules.Purchasing.Domain.Enums;
using PrismCuisine.Modules.Purchasing.IntegrationEvents;

namespace PrismCuisine.Modules.Purchasing.Application.PurchaseOrders;

public sealed class PurchaseOrderService(
    IPurchasingUnitOfWork unitOfWork,
    IIntegrationEventPublisher eventPublisher) : IPurchaseOrderService
{
    public Task<IReadOnlyCollection<PurchaseOrderSummaryDto>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        unitOfWork.PurchaseOrders.GetAllAsync(cancellationToken);

    public Task<PurchaseOrderDto?> GetByIdAsync(int purchaseOrderId, CancellationToken cancellationToken = default) =>
        unitOfWork.PurchaseOrders.GetByIdWithLinesAsync(purchaseOrderId, cancellationToken);

    public async Task<PurchaseOrderDto> CreateAsync(
        CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = await unitOfWork.Suppliers.GetByIdAsync(request.SupplierId, cancellationToken)
            ?? throw new DomainException($"Supplier '{request.SupplierId}' was not found.");

        if (request.Lines is null || request.Lines.Count == 0)
        {
            throw new DomainException("Purchase order must have at least one line.");
        }

        var orderNumber = await GenerateOrderNumberAsync(cancellationToken);
        var order = PurchaseOrder.CreateDraft(
            orderNumber,
            request.SupplierId,
            request.WarehouseId,
            request.Notes);

        foreach (var line in request.Lines)
        {
            order.AddLine(line.ProductId, line.QuantityOrdered, line.UnitPrice);
        }

        unitOfWork.PurchaseOrders.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return (await unitOfWork.PurchaseOrders.GetByIdWithLinesAsync(order.Id, cancellationToken))!;
    }

    public async Task UpdateAsync(
        int purchaseOrderId,
        UpdatePurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(purchaseOrderId, cancellationToken)
            ?? throw new DomainException($"Purchase order '{purchaseOrderId}' was not found.");

        if (request.Lines is null || request.Lines.Count == 0)
        {
            throw new DomainException("Purchase order must have at least one line.");
        }

        _ = await unitOfWork.Suppliers.GetByIdAsync(request.SupplierId, cancellationToken)
            ?? throw new DomainException($"Supplier '{request.SupplierId}' was not found.");

        order.UpdateDraft(request.SupplierId, request.WarehouseId, request.Notes);
        order.ReplaceLines(request.Lines
            .Select(l => (l.ProductId, l.QuantityOrdered, l.UnitPrice))
            .ToList());

        unitOfWork.PurchaseOrders.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PurchaseOrderDto> CreateAmendmentAsync(
        int purchaseOrderId,
        CreatePurchaseOrderAmendmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var source = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(purchaseOrderId, cancellationToken)
            ?? throw new DomainException($"Purchase order '{purchaseOrderId}' was not found.");

        if (source.Status is not PurchaseOrderStatus.Approved and not PurchaseOrderStatus.PartiallyReceived)
        {
            throw new DomainException("Only approved or partially received purchase orders can be amended.");
        }

        var orderNumber = await GenerateOrderNumberAsync(cancellationToken);
        var amendment = PurchaseOrder.CreateAmendment(orderNumber, source, request.Notes);

        if (request.Lines is { Count: > 0 })
        {
            foreach (var line in request.Lines)
            {
                amendment.AddLine(line.ProductId, line.QuantityOrdered, line.UnitPrice);
            }
        }
        else if (request.CopyRemainingLines)
        {
            foreach (var line in source.Lines.Where(l => l.QuantityRemaining > 0))
            {
                amendment.AddLine(line.ProductId, line.QuantityRemaining, line.UnitPrice);
            }
        }

        if (amendment.Lines.Count == 0)
        {
            throw new DomainException("Amendment must include at least one line.");
        }

        unitOfWork.PurchaseOrders.Add(amendment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return (await unitOfWork.PurchaseOrders.GetByIdWithLinesAsync(amendment.Id, cancellationToken))!;
    }

    public async Task AddLineAsync(
        int purchaseOrderId,
        AddPurchaseOrderLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(purchaseOrderId, cancellationToken)
            ?? throw new DomainException($"Purchase order '{purchaseOrderId}' was not found.");

        order.AddLine(request.ProductId, request.QuantityOrdered, request.UnitPrice);
        unitOfWork.PurchaseOrders.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveAsync(int purchaseOrderId, CancellationToken cancellationToken = default)
    {
        var order = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(purchaseOrderId, cancellationToken)
            ?? throw new DomainException($"Purchase order '{purchaseOrderId}' was not found.");

        order.Approve();
        unitOfWork.PurchaseOrders.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        //await eventPublisher.PublishAsync(
        //    new PurchaseOrderApprovedIntegrationEvent(order.Id, order.OrderNumber),
        //    cancellationToken);
    }

    public async Task CancelAsync(int purchaseOrderId, CancellationToken cancellationToken = default)
    {
        var order = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(purchaseOrderId, cancellationToken)
            ?? throw new DomainException($"Purchase order '{purchaseOrderId}' was not found.");

        order.Cancel();
        unitOfWork.PurchaseOrders.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var count = await unitOfWork.PurchaseOrders.GetCountForDateAsync(today, cancellationToken);
        return $"PO-{today:yyyyMMdd}-{(count + 1):D4}";
    }
}
