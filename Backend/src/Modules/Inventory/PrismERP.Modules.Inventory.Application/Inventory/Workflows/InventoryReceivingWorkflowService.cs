using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Application.Inventory.Internal;
using PrismERP.Modules.Inventory.Domain.Entities;
using PrismERP.Modules.Inventory.Domain.Enums;

namespace PrismERP.Modules.Inventory.Application.Inventory.Workflows;

public sealed class InventoryReceivingWorkflowService(
    IInventoryUnitOfWork unitOfWork,
    InventoryBalanceAccess balanceAccess) : IInventoryReceivingWorkflowService
{
    public async Task<InventoryMovement> ReceiveStockAsync(
        ReceiveInventoryRequest request,
        CancellationToken cancellationToken = default)
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
}
