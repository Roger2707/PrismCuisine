using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.Inventory.Application.Inventory;
using PrismCuisine.Modules.Purchasing.Application.Abstractions;
using PrismCuisine.Modules.Purchasing.Domain.Entities;
using PrismCuisine.Modules.Purchasing.Domain.Enums;

namespace PrismCuisine.Modules.Purchasing.Application.GoodsReceipts;

public sealed class GoodsReceiptService(
    IPurchasingUnitOfWork unitOfWork,
    IInventoryPostingService inventoryPosting) : IGoodsReceiptService
{
    public Task<IReadOnlyCollection<GoodsReceiptSummaryDto>> GetByPurchaseOrderIdAsync(
        int purchaseOrderId,
        CancellationToken cancellationToken = default) =>
        unitOfWork.GoodsReceipts.GetByPurchaseOrderIdAsync(purchaseOrderId, cancellationToken);

    public Task<GoodsReceiptDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        unitOfWork.GoodsReceipts.GetByIdWithLinesAsync(id, cancellationToken);

    public async Task<GoodsReceiptDto> CreateAsync(
        CreateGoodsReceiptRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Lines is null || request.Lines.Count == 0)
        {
            throw new DomainException("Goods receipt must contain at least one line.");
        }

        var order = await GetOrderForReceivingAsync(request.PurchaseOrderId, cancellationToken);

        var receiptNumber = await GenerateReceiptNumberAsync(cancellationToken);
        var receipt = GoodsReceipt.CreateDraft(request.PurchaseOrderId, receiptNumber, request.Notes);

        foreach (var line in request.Lines)
        {
            AddLineCore(receipt, order, line);
        }

        unitOfWork.GoodsReceipts.Add(receipt);

        if (request.PostImmediately)
        {
            await PostCoreAsync(receipt, order, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return (await unitOfWork.GoodsReceipts.GetByIdWithLinesAsync(receipt.Id, cancellationToken))!;
    }

    public async Task UpdateAsync(
        int goodsReceiptId,
        UpdateGoodsReceiptRequest request,
        CancellationToken cancellationToken = default)
    {
        var receipt = await unitOfWork.GoodsReceipts.GetByIdWithLinesForUpdateAsync(goodsReceiptId, cancellationToken)
            ?? throw new DomainException($"Goods receipt '{goodsReceiptId}' was not found.");

        if (request.Lines is null || request.Lines.Count == 0)
        {
            throw new DomainException("Goods receipt must contain at least one line.");
        }

        var order = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(
            receipt.PurchaseOrderId,
            cancellationToken)
            ?? throw new DomainException("Purchase order for goods receipt was not found.");

        if (order.Status is not PurchaseOrderStatus.Approved and not PurchaseOrderStatus.PartiallyReceived)
        {
            throw new DomainException("Goods receipt can only be updated for approved purchase orders.");
        }

        var preparedLines = request.Lines.Select(line =>
        {
            var poLine = order.Lines.FirstOrDefault(l => l.Id == line.PurchaseOrderLineId)
                ?? throw new DomainException($"Purchase order line '{line.PurchaseOrderLineId}' was not found.");

            if (line.Quantity > poLine.QuantityRemaining)
            {
                throw new DomainException(
                    $"Receipt quantity '{line.Quantity}' exceeds remaining PO quantity '{poLine.QuantityRemaining}' for line '{poLine.Id}'.");
            }

            var unitCost = line.UnitCost ?? poLine.UnitPrice;
            return (line.PurchaseOrderLineId, poLine.ProductId, line.Quantity, unitCost);
        }).ToList();

        receipt.UpdateDraft(request.Notes);
        receipt.ReplaceLines(preparedLines);
        unitOfWork.GoodsReceipts.Update(receipt);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AddLineAsync(
        int goodsReceiptId,
        AddGoodsReceiptLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var receipt = await unitOfWork.GoodsReceipts.GetByIdWithLinesForUpdateAsync(goodsReceiptId, cancellationToken)
            ?? throw new DomainException($"Goods receipt '{goodsReceiptId}' was not found.");

        var order = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(
            receipt.PurchaseOrderId,
            cancellationToken)
            ?? throw new DomainException("Purchase order for goods receipt was not found.");

        AddLineCore(receipt, order, request);
        unitOfWork.GoodsReceipts.Update(receipt);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<GoodsReceiptDto> PostAsync(int goodsReceiptId, CancellationToken cancellationToken = default)
    {
        var receipt = await unitOfWork.GoodsReceipts.GetByIdWithLinesForUpdateAsync(goodsReceiptId, cancellationToken)
            ?? throw new DomainException($"Goods receipt '{goodsReceiptId}' was not found.");

        var order = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(
            receipt.PurchaseOrderId,
            cancellationToken)
            ?? throw new DomainException("Purchase order for goods receipt was not found.");

        await PostCoreAsync(receipt, order, cancellationToken);
        unitOfWork.GoodsReceipts.Update(receipt);
        unitOfWork.PurchaseOrders.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return (await unitOfWork.GoodsReceipts.GetByIdWithLinesAsync(receipt.Id, cancellationToken))!;
    }

    private async Task<string> GenerateReceiptNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var count = await unitOfWork.GoodsReceipts.GetCountForDateAsync(today, cancellationToken);
        return $"GRN-{today:yyyyMMdd}-{(count + 1):D4}";
    }

    private static void AddLineCore(
        GoodsReceipt receipt,
        PurchaseOrder order,
        AddGoodsReceiptLineRequest request)
    {
        var poLine = order.Lines.FirstOrDefault(l => l.Id == request.PurchaseOrderLineId)
            ?? throw new DomainException($"Purchase order line '{request.PurchaseOrderLineId}' was not found.");

        if (request.Quantity > poLine.QuantityRemaining)
        {
            throw new DomainException(
                $"Receipt quantity '{request.Quantity}' exceeds remaining PO quantity '{poLine.QuantityRemaining}' for line '{poLine.Id}'.");
        }

        var unitCost = request.UnitCost ?? poLine.UnitPrice;
        receipt.AddLine(request.PurchaseOrderLineId, poLine.ProductId, request.Quantity, unitCost);
    }

    private async Task PostCoreAsync(
        GoodsReceipt receipt,
        PurchaseOrder order,
        CancellationToken cancellationToken)
    {
        foreach (var line in receipt.Lines)
        {
            order.RecordReceipt(line.PurchaseOrderLineId, line.Quantity);

            await inventoryPosting.ReceiveAsync(
                new ReceiveInventoryRequest(
                    line.ProductId,
                    order.WarehouseId,
                    line.Quantity,
                    line.UnitCost,
                    receipt.ReceiptNumber,
                    receipt.PurchaseOrderId,
                    $"GRN line {line.Id}"),
                cancellationToken);
        }

        receipt.Post();
    }

    private async Task<PurchaseOrder> GetOrderForReceivingAsync(
        int purchaseOrderId,
        CancellationToken cancellationToken)
    {
        var order = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(
            purchaseOrderId,
            cancellationToken)
            ?? throw new DomainException($"Purchase order '{purchaseOrderId}' was not found.");

        if (order.Status is not PurchaseOrderStatus.Approved and not PurchaseOrderStatus.PartiallyReceived)
        {
            throw new DomainException("Goods receipt can only be created for approved purchase orders.");
        }

        return order;
    }
}
