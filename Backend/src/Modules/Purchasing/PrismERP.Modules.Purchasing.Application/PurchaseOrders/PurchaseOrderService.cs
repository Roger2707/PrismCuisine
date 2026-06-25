using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Purchasing.Application.Abstractions;
using PrismERP.Modules.Purchasing.Domain.Entities;
using PrismERP.Modules.Purchasing.Domain.Enums;

namespace PrismERP.Modules.Purchasing.Application.PurchaseOrders;

public sealed class PurchaseOrderService(IPurchasingUnitOfWork unitOfWork) : IPurchaseOrderService
{
    #region Read

    public Task<IReadOnlyCollection<PurchaseOrderSummaryDto>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        unitOfWork.PurchaseOrders.GetAllAsync(cancellationToken);

    public Task<PurchaseOrderDto?> GetByIdAsync(int purchaseOrderId, CancellationToken cancellationToken = default) =>
        unitOfWork.PurchaseOrders.GetByIdWithLinesAsync(purchaseOrderId, cancellationToken);

    #endregion

    #region Write

    public async Task<PurchaseOrderDto> CreateAsync(
        CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = await unitOfWork.Suppliers.GetByIdAsync(request.SupplierId, cancellationToken)
            ?? throw new NotFoundException($"Supplier '{request.SupplierId}' was not found.");

        if (request.Lines is null || request.Lines.Count == 0)
        {
            throw new BusinessException("Purchase order must have at least one line.");
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
            ?? throw new NotFoundException($"Purchase order '{purchaseOrderId}' was not found.");

        if (request.Lines is null || request.Lines.Count == 0)
        {
            throw new BusinessException("Purchase order must have at least one line.");
        }

        _ = await unitOfWork.Suppliers.GetByIdAsync(request.SupplierId, cancellationToken)
            ?? throw new NotFoundException($"Supplier '{request.SupplierId}' was not found.");

        order.UpdateDraft(request.SupplierId, request.WarehouseId, request.Notes);
        order.ReplaceLines(request.Lines
            .Select(l => (l.ProductId, l.QuantityOrdered, l.UnitPrice))
            .ToList());

        unitOfWork.PurchaseOrders.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Amendment

    public async Task<PurchaseOrderDto> CreateAmendmentAsync(
    int purchaseOrderId,
    CreatePurchaseOrderAmendmentRequest request,
    CancellationToken cancellationToken = default)
    {
        var source = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(purchaseOrderId, cancellationToken)
            ?? throw new NotFoundException($"Purchase order '{purchaseOrderId}' was not found.");

        if (source.Status is not PurchaseOrderStatus.Approved and not PurchaseOrderStatus.PartiallyReceived)
        {
            throw new BusinessException("Only approved or partially received purchase orders can be amended.");
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
            throw new BusinessException("Amendment must include at least one line.");
        }

        unitOfWork.PurchaseOrders.Add(amendment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return (await unitOfWork.PurchaseOrders.GetByIdWithLinesAsync(amendment.Id, cancellationToken))!;
    }

    #endregion

    #region Approve

    public async Task ApproveAsync(int purchaseOrderId, CancellationToken cancellationToken = default)
    {
        await unitOfWork.ExecuteInTransactionWithRetryAsync(async ct =>
        {
            var po = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(purchaseOrderId, ct)
                ?? throw new NotFoundException($"Purchase order '{purchaseOrderId}' was not found.");

            // Rule: Only Draft status can be Approved 
            // If double Approve this case show Approved => OK
            if (po.Status != PurchaseOrderStatus.Draft)
                throw new ConflictException(
                    $"Sales order '{po.OrderNumber}' is already '{po.Status}'. Refresh and try again.");

            po.Approve();
            unitOfWork.PurchaseOrders.Update(po);
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);
    }

    #endregion

    #region Cancel

    public async Task CancelAsync(int purchaseOrderId, CancellationToken cancellationToken = default)
    {
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var po = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(purchaseOrderId, ct)
                ?? throw new NotFoundException($"Purchase order '{purchaseOrderId}' was not found.");

            // Draft => Cancel, PartialReceived or Received => cancel at GR
            if(po.Status == PurchaseOrderStatus.Approved)
            {
                // 1. Find GRs
                var goodReceipts = await unitOfWork.GoodsReceipts.GetByPurchaseOrderIdForUpdateAsync(purchaseOrderId, ct);
                if(goodReceipts != null && goodReceipts.Count > 0)
                {
                    if (goodReceipts.Any(r => r.Status == GoodsReceiptStatus.Posted))
                        throw new BusinessException("GoodsReceipt posted cannot be Cancel, let's handle in GR Cancel !");

                    // delete all GoodsReceipts in DB if Draft status
                    unitOfWork.GoodsReceipts.DeleteRange(goodReceipts);
                }

                // 2. Check PO Line 's ReceivedQty
                if (po.Lines.Any(l => l.QuantityReceived > 0))
                    throw new BusinessException($"There are at least 1 PO Line has receive qty ! this case have to no receivedQty");
            }

            po.Cancel();
            unitOfWork.PurchaseOrders.Update(po);
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);
    }

    #endregion

    #region Helpers

    private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var count = await unitOfWork.PurchaseOrders.GetCountForDateAsync(today, cancellationToken);
        return $"PO-{today:yyyyMMdd}-{(count + 1):D4}";
    }

    #endregion
}
