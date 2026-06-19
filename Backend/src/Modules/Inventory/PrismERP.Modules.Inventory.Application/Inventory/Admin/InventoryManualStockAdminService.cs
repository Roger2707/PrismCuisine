using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Application.Inventory.Internal;
using PrismERP.Modules.Inventory.Application.Inventory.Mapping;
using PrismERP.Modules.Inventory.Application.Inventory.Workflows;
using PrismERP.Modules.Inventory.Domain.Entities;
using PrismERP.Modules.Inventory.Domain.Enums;

namespace PrismERP.Modules.Inventory.Application.Inventory.Admin;

public sealed class InventoryManualStockAdminService(
    IInventoryUnitOfWork unitOfWork,
    IInventoryReceivingWorkflowService receivingWorkflow,
    InventoryBalanceAccess balanceAccess,
    InventoryAvailabilityChecker availabilityChecker,
    InventoryFifoIssuer fifoIssuer) : IInventoryManualStockAdminService
{
    public async Task<InventoryMovementDto> ReceiveAsync(
        ReceiveInventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var movement = await receivingWorkflow.ReceiveStockAsync(request, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return InventoryDtoMapper.ToMovementDto(movement);
    }

    public async Task<List<InventoryMovementDto>> IssueAsync(
        IssueInventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var balance = await balanceAccess.GetForUpdateByProductWarehouseAsync(
            request.ProductId,
            request.WarehouseId,
            cancellationToken);

        await availabilityChecker.EnsureAvailableAsync(balance.Id, request.Quantity, cancellationToken);

        var movements = await fifoIssuer.IssueFromBalanceAsync(
            balance,
            request.Quantity,
            InventoryReferenceType.Manual,
            request.Reference,
            request.ReferenceId,
            request.Notes,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return movements.Select(InventoryDtoMapper.ToMovementDto).ToList();
    }

    public async Task<List<InventoryMovementDto>> AdjustAsync(
        AdjustInventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var balance = await balanceAccess.GetForUpdateByProductWarehouseAsync(
            request.ProductId,
            request.WarehouseId,
            cancellationToken);

        if (request.NewQuantity < 0)
        {
            throw new BusinessException("Adjusted quantity cannot be negative.");
        }

        var delta = request.NewQuantity - balance.QuantityOnHand;
        if (delta == 0)
        {
            throw new BusinessException("Adjusted quantity is the same as current on-hand quantity.");
        }

        List<InventoryMovement> movements;

        if (delta > 0)
        {
            var layer = InventoryCostLayer.Create(balance.Id, delta, request.UnitCostForIncrease);
            unitOfWork.CostLayers.Add(layer);
            balance.Increase(delta);
            unitOfWork.Balances.Update(balance);

            movements =
            [
                InventoryMovement.Create(
                    balance.Id,
                    InventoryMovementType.Adjustment,
                    delta,
                    request.UnitCostForIncrease,
                    InventoryReferenceType.Adjustment,
                    layer.Id,
                    request.Reference,
                    referenceId: null,
                    request.Notes)
            ];

            foreach (var movement in movements)
            {
                unitOfWork.Movements.Add(movement);
            }
        }
        else
        {
            var issueQty = Math.Abs(delta);
            await availabilityChecker.EnsureAvailableAsync(balance.Id, issueQty, cancellationToken);
            movements = await fifoIssuer.IssueFromBalanceAsync(
                balance,
                issueQty,
                InventoryReferenceType.Adjustment,
                request.Reference,
                referenceId: null,
                request.Notes,
                cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return movements.Select(InventoryDtoMapper.ToMovementDto).ToList();
    }
}
