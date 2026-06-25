using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Finance.Application.Invoices;
using PrismERP.Modules.Inventory.Application.Inventory;
using PrismERP.Modules.Inventory.Application.Inventory.Workflows;
using PrismERP.Modules.Inventory.Domain.Entities;
using PrismERP.Modules.Purchasing.Application.Abstractions;
using PrismERP.Modules.Purchasing.Domain.Entities;
using PrismERP.Modules.Purchasing.Domain.Enums;

namespace PrismERP.Modules.Purchasing.Application.GoodsReceipts;

public sealed class GoodsReceiptService(
    IPurchasingUnitOfWork unitOfWork,
    IInventoryReceivingWorkflowService inventoryReceiving,
    IInvoiceService invoiceService) : IGoodsReceiptService
{
    #region Read

    public Task<IReadOnlyCollection<GoodsReceiptSummaryDto>> GetByPurchaseOrderIdAsync(
        int purchaseOrderId,
        CancellationToken cancellationToken = default) =>
        unitOfWork.GoodsReceipts.GetByPurchaseOrderIdAsync(purchaseOrderId, cancellationToken);

    public Task<GoodsReceiptDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        unitOfWork.GoodsReceipts.GetByIdWithLinesAsync(id, cancellationToken);

    #endregion

    #region Write

    public async Task<GoodsReceiptDto> CreateAsync(CreateGoodsReceiptRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Lines is null || request.Lines.Count == 0)
            throw new BusinessException("Goods receipt must contain at least one line.");

        // Check PurchaseOrder
        var po = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(request.PurchaseOrderId, cancellationToken)
            ?? throw new BusinessException($"Can not find PurchaseOrder with ID: {request.PurchaseOrderId} !");

        if (po.Status is not PurchaseOrderStatus.Approved and not PurchaseOrderStatus.PartiallyReceived)
            throw new BusinessException("Goods receipt can only be created for approved purchase orders.");

        var receiptNumber = await GenerateReceiptNumberAsync(cancellationToken);
        var receipt = GoodsReceipt.CreateDraft(request.PurchaseOrderId, receiptNumber, request.Notes);

        foreach (var line in request.Lines)
            AddLineCore(receipt, po, line);

        unitOfWork.GoodsReceipts.Add(receipt);
        if (request.PostImmediately)
        {
            await unitOfWork.ExecuteInTransactionWithRetryAsync(async ct =>
            {
                await PostCoreAsync(receipt, po, ct);
                unitOfWork.GoodsReceipts.Update(receipt);
                unitOfWork.PurchaseOrders.Update(po);
                await unitOfWork.SaveChangesAsync(ct);

            }, cancellationToken);

            return (await unitOfWork.GoodsReceipts.GetByIdWithLinesAsync(receipt.Id, cancellationToken))!;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return (await unitOfWork.GoodsReceipts.GetByIdWithLinesAsync(receipt.Id, cancellationToken))!;
    }

    public async Task UpdateAsync(int goodsReceiptId, UpdateGoodsReceiptRequest request, CancellationToken cancellationToken = default)
    {
        var receipt = await unitOfWork.GoodsReceipts.GetByIdWithLinesForUpdateAsync(goodsReceiptId, cancellationToken)
            ?? throw new NotFoundException($"Goods receipt '{goodsReceiptId}' was not found.");

        if (request.Lines is null || request.Lines.Count == 0)
            throw new BusinessException("Goods receipt must contain at least one line.");

        var po = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(
            receipt.PurchaseOrderId, cancellationToken) 
            ?? throw new NotFoundException("Purchase order for goods receipt was not found.");

        if (po.Status is not PurchaseOrderStatus.Approved and not PurchaseOrderStatus.PartiallyReceived)
            throw new BusinessException("Goods receipt can only be updated for approved purchase orders.");

        var preparedLines = request.Lines.Select(line =>
        {
            var poLine = po.Lines.FirstOrDefault(l => l.Id == line.PurchaseOrderLineId)
                ?? throw new BusinessException($"Purchase order line '{line.PurchaseOrderLineId}' was not found.");

            if (line.Quantity > poLine.QuantityRemaining)
            {
                throw new BusinessException(
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

    #endregion

    #region Post

    public async Task<GoodsReceiptDto> PostAsync(int goodsReceiptId, CancellationToken cancellationToken = default)
    {
        await unitOfWork.ExecuteInTransactionWithRetryAsync(async ct =>
        {
            var receipt = await unitOfWork.GoodsReceipts.GetByIdWithLinesForUpdateAsync(goodsReceiptId, ct)
                ?? throw new NotFoundException($"Goods receipt '{goodsReceiptId}' was not found.");

            // double Post (2 user click concurrency) || draft status can be Post
            if (receipt.Status != GoodsReceiptStatus.Draft)
                throw new ConflictException(
                    $"GoodsReceipt note '{receipt.ReceiptNumber}' is already '{receipt.Status}'. Refresh and try again.");

            var po = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(receipt.PurchaseOrderId, ct)
                ?? throw new NotFoundException("Purchase order for goods receipt was not found.");

            await PostCoreAsync(receipt, po, ct);
            unitOfWork.GoodsReceipts.Update(receipt);
            unitOfWork.PurchaseOrders.Update(po);
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        return (await unitOfWork.GoodsReceipts.GetByIdWithLinesAsync(goodsReceiptId, cancellationToken))!;
    }

    private async Task PostCoreAsync(GoodsReceipt receipt, PurchaseOrder po, CancellationToken cancellationToken)
    {
        foreach (var line in receipt.Lines)
        {
            po.RecordReceipt(line.PurchaseOrderLineId, line.Quantity);

            await inventoryReceiving.ReceiveStockAsync(
                new ReceiveInventoryRequest(
                    line.ProductId,
                    po.WarehouseId,
                    line.Quantity,
                    line.UnitCost,
                    receipt.ReceiptNumber,
                    receipt.PurchaseOrderId,
                    $"GRN line {line.Id}"),
                cancellationToken);
        }
        receipt.Post();
    }

    #endregion

    #region Cancel

    public async Task CancelAsync(int goodsReceiptId, CancellationToken cancellationToken = default)
    {
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            GoodsReceipt goodsReceipt = await unitOfWork.GoodsReceipts.GetByIdWithLinesForUpdateAsync(goodsReceiptId, ct)
                ?? throw new NotFoundException($"GoodsReceipt with ID : {goodsReceiptId} is not found !");

            var purchaseOrder = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(goodsReceipt.PurchaseOrderId, ct)
                ?? throw new NotFoundException($"PurchaseOrder with ID : {goodsReceipt.PurchaseOrderId} is not found !");

            var returnLines = goodsReceipt.Lines
                    .Select(l => new ReturnGoodsReceiptLine(purchaseOrder.Id, l.Quantity))
                    .ToList();

            // Return export stock ....
            await inventoryReceiving.ReturnGoodsReceiptAsync(goodsReceipt.ReceiptNumber, returnLines, ct);

            // Cancel
            goodsReceipt.Cancel(purchaseOrder);

            // update status
            await UpdatePurchaseInvoiceStatus(purchaseOrder);

            // Cancel Invoice has been created
            var invoiceDto = await invoiceService.GetByGoodsReceiptIdAsync(goodsReceiptId, ct);
            if (invoiceDto is not null)
                await invoiceService.CancelAsync(invoiceDto.Id, ct);

            // update entities
            unitOfWork.PurchaseOrders.Update(purchaseOrder);
            unitOfWork.GoodsReceipts.Update(goodsReceipt);

            await unitOfWork.SaveChangesAsync(ct);

        }, cancellationToken);
    }

    private async Task UpdatePurchaseInvoiceStatus(PurchaseOrder purchaseOrder)
    {
        var invoices = await invoiceService.GetInvoicesByPurchaseOrderAsync(purchaseOrder.Id);
        if (invoices == null || !invoices.Any() || purchaseOrder.Lines == null || !purchaseOrder.Lines.Any())
            return;

        var totalInvoicedQtyByProduct = invoices
                .SelectMany(inv => inv.Lines)
                .GroupBy(invLine => invLine.ProductId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Sum(invLine => invLine.Quantity)
                );

        bool isAllLinesFullyInvoiced = true;
        foreach (var pLine in purchaseOrder.Lines)
        {
            totalInvoicedQtyByProduct.TryGetValue(pLine.ProductId, out decimal totalInvoicedQty);
            if (totalInvoicedQty != pLine.QuantityOrdered)
            {
                isAllLinesFullyInvoiced = false;
                break;
            }
        }

        purchaseOrder.UpdateInvoiceStatus(
            isAllLinesFullyInvoiced ?
            PurchaseOrderInvoicingStatus.FullyInvoiced : PurchaseOrderInvoicingStatus.PartiallyInvoiced
        );
    }

    #endregion

    #region Helpers

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
            ?? throw new NotFoundException($"Purchase order line '{request.PurchaseOrderLineId}' was not found.");

        if (request.Quantity > poLine.QuantityRemaining)
        {
            throw new BusinessException(
                $"Receipt quantity '{request.Quantity}' exceeds remaining PO quantity '{poLine.QuantityRemaining}' for line '{poLine.Id}'.");
        }

        var unitCost = request.UnitCost ?? poLine.UnitPrice;
        receipt.AddLine(request.PurchaseOrderLineId, poLine.ProductId, request.Quantity, unitCost);
    }

    #endregion

}
