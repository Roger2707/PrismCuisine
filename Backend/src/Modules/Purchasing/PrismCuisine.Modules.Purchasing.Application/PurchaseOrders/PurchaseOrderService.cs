using PrismCuisine.BuildingBlocks.Application.Abstractions.Messaging;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.Purchasing.Application.Abstractions;
using PrismCuisine.Modules.Purchasing.Domain.Entities;
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

        var orderNumber = await GenerateOrderNumberAsync(cancellationToken);
        var order = PurchaseOrder.CreateDraft(
            orderNumber,
            request.SupplierId,
            request.WarehouseId,
            request.Notes);

        unitOfWork.PurchaseOrders.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return (await unitOfWork.PurchaseOrders.GetByIdWithLinesAsync(order.Id, cancellationToken))!;
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

        await eventPublisher.PublishAsync(
            new PurchaseOrderApprovedIntegrationEvent(order.Id, order.OrderNumber),
            cancellationToken);
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
