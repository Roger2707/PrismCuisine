using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Domain.Entities;
using PrismERP.Modules.Inventory.Domain.Enums;
using PrismERP.Modules.Inventory.Domain.Services;

namespace PrismERP.Modules.Inventory.Application.Inventory.Internal;

public sealed class InventoryFifoIssuer(IInventoryUnitOfWork unitOfWork)
{
    public async Task<List<InventoryMovement>> IssueFromBalanceAsync(
        InventoryBalance balance,
        decimal quantity,
        InventoryReferenceType referenceType,
        string? reference,
        int? referenceId,
        string? notes,
        CancellationToken cancellationToken)
    {
        var layers = (await unitOfWork.CostLayers.GetAvailableLayersForUpdateAsync(balance.Id, cancellationToken)).ToList();
        return IssueFromBalance(balance, quantity, layers, referenceType, reference, referenceId, notes);
    }

    public List<InventoryMovement> IssueFromBalance(
        InventoryBalance balance,
        decimal quantity,
        List<InventoryCostLayer> layers,
        InventoryReferenceType referenceType,
        string? reference,
        int? referenceId,
        string? notes)
    {
        var movements = new List<InventoryMovement>();
        var consumptions = FifoCosting.Consume(layers, quantity);

        var consumedLayerIds = consumptions.Select(c => c.CostLayerId).ToHashSet();
        foreach (var layer in layers.Where(l => consumedLayerIds.Contains(l.Id)))
        {
            unitOfWork.CostLayers.Update(layer);
        }

        balance.Decrease(quantity);
        unitOfWork.Balances.Update(balance);

        foreach (var consumption in consumptions)
        {
            var layer = layers.First(l => l.Id == consumption.CostLayerId);
            var movement = InventoryMovement.Create(
                balance.Id,
                InventoryMovementType.Issue,
                consumption.Quantity,
                layer.UnitCost,
                referenceType,
                layer.Id,
                reference,
                referenceId,
                notes);

            unitOfWork.Movements.Add(movement);
            movements.Add(movement);
        }

        return movements;
    }
}
