using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Application.Inventory.Internal;
using PrismERP.Modules.Inventory.Domain.Entities;
using PrismERP.Modules.Inventory.Domain.Enums;

namespace PrismERP.Modules.Inventory.Application.Inventory.Workflows;

public sealed class InventoryReceivingWorkflowService(IInventoryUnitOfWork unitOfWork, InventoryBalanceAccess balanceAccess) : IInventoryReceivingWorkflowService
{
    public async Task<InventoryMovement> ReceiveStockAsync(ReceiveInventoryRequest request, CancellationToken cancellationToken = default)
    {
        var balance = await balanceAccess.GetOrCreateForUpdateAsync(
            request.ProductId,
            request.WarehouseId,
            cancellationToken);

        var layer = InventoryCostLayer.Create(balance.Id, request.Quantity, request.UnitCost);
        unitOfWork.CostLayers.Add(layer);

        balance.Increase(request.Quantity);
        unitOfWork.Balances.Update(balance);

        var movement = InventoryMovement.Create(
            balance.Id,
            InventoryMovementType.Receipt,
            request.Quantity,
            request.UnitCost,
            InventoryReferenceType.Manual,
            layer.Id,
            request.Reference,
            request.ReferenceId,
            request.Notes);

        unitOfWork.Movements.Add(movement);
        return movement;
    }

    #region Return GoodsReceipt

    public async Task ReturnGoodsReceiptAsync(string goodsReceiptNumber, IReadOnlyList<ReturnGoodsReceiptLine> lines, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(goodsReceiptNumber))
            throw new BusinessException("GoodsReceipt number is required.");

        if (lines.Count == 0)
            throw new BusinessException("At least one goodsReceipt line is required to return stock.");

        // Load data in history
        var referenceIds = lines.Select(l => l.PurchaseOrderLineId).ToHashSet();
        var receiptMovements = await unitOfWork.Movements.GetReceiptByPurchaseOrderReferenceAsync(
            InventoryReferenceType.Manual,
            goodsReceiptNumber.Trim(),
            referenceIds,
            cancellationToken);

        if (receiptMovements.Count == 0)
            throw new BusinessException($"No receipt movements found for GoodsReceipt '{goodsReceiptNumber}'.");

        var movementsByLine = receiptMovements
            .GroupBy(m => m.ReferenceId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var line in lines)
        {
            if (!movementsByLine.TryGetValue(line.PurchaseOrderLineId, out var lineMovements))
            {
                throw new BusinessException(
                    $"No receipt movements found for purchase order line '{line.PurchaseOrderLineId}' on delivery '{goodsReceiptNumber}'.");
            }

            var issuedQty = lineMovements.Sum(m => m.Quantity);
            if (issuedQty != line.Quantity)
            {
                throw new BusinessException(
                    $"Return quantity '{line.Quantity}' does not match received quantity '{issuedQty}' for purchase order line '{line.PurchaseOrderLineId}'.");
            }
        }

        // Get CostLayer have been received
        var layerIds = receiptMovements
            .Select(m => m.InventoryCostLayerId)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        var layers = await unitOfWork.CostLayers.GetByIdsForUpdateAsync(layerIds, cancellationToken);
        var layerById = layers.ToDictionary(l => l.Id);

        var balanceIds = receiptMovements.Select(m => m.InventoryBalanceId).Distinct().ToList();
        var balances = await unitOfWork.Balances.GetByIdsForUpdateAsync(balanceIds, cancellationToken);
        var balanceById = balances.ToDictionary(b => b.Id);

        foreach (var movement in receiptMovements)
        {
            if (!layerById.TryGetValue(movement.InventoryCostLayerId, out var layer))
            {
                throw new NotFoundException($"Cost layer '{movement.InventoryCostLayerId}' was not found.");
            }

            if (!balanceById.TryGetValue(movement.InventoryBalanceId, out var balance))
            {
                throw new NotFoundException($"Inventory balance '{movement.InventoryBalanceId}' was not found.");
            }

            // minus qty (cost layer + balance)
            layer.Consume(movement.Quantity);
            unitOfWork.CostLayers.Update(layer);

            balance.Decrease(movement.Quantity);
            unitOfWork.Balances.Update(balance);

            var returnMovement = InventoryMovement.Create(
                balance.Id,
                InventoryMovementType.Return,
                movement.Quantity,
                movement.UnitCost,
                InventoryReferenceType.PurchaseOrder,
                layer.Id,
                goodsReceiptNumber.Trim(),
                movement.ReferenceId,
                $"Return from cancelled delivery {goodsReceiptNumber.Trim()}");

            unitOfWork.Movements.Add(returnMovement);
        }
    }

    #endregion
}
