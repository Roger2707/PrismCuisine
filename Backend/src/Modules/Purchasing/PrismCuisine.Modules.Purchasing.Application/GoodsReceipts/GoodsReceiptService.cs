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
        var order = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(
            request.PurchaseOrderId,
            cancellationToken)
            ?? throw new DomainException($"Purchase order '{request.PurchaseOrderId}' was not found.");

        if (order.Status is not PurchaseOrderStatus.Approved and not PurchaseOrderStatus.PartiallyReceived)
        {
            throw new DomainException("Goods receipt can only be created for approved purchase orders.");
        }

        var receiptNumber = await GenerateReceiptNumberAsync(cancellationToken);
        var receipt = GoodsReceipt.CreateDraft(request.PurchaseOrderId, receiptNumber, request.Notes);

        unitOfWork.GoodsReceipts.Add(receipt);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return (await unitOfWork.GoodsReceipts.GetByIdWithLinesAsync(receipt.Id, cancellationToken))!;
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

        var poLine = order.Lines.FirstOrDefault(l => l.Id == request.PurchaseOrderLineId)
            ?? throw new DomainException($"Purchase order line '{request.PurchaseOrderLineId}' was not found.");

        var unitCost = request.UnitCost ?? poLine.UnitPrice;

        receipt.AddLine(request.PurchaseOrderLineId, poLine.ProductId, request.Quantity, unitCost);
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
}
