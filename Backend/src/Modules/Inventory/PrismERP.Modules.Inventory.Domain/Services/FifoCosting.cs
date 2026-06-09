using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Inventory.Domain.Entities;

namespace PrismERP.Modules.Inventory.Domain.Services;

public sealed record LayerConsumption(int CostLayerId, decimal Quantity, decimal UnitCost);

public static class FifoCosting
{
    public static IReadOnlyList<LayerConsumption> Consume(
        IReadOnlyCollection<InventoryCostLayer> layersOldestFirst,
        decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Issue quantity must be greater than zero.");
        }

        var remaining = quantity;
        var consumptions = new List<LayerConsumption>();

        foreach (var layer in layersOldestFirst)
        {
            if (remaining <= 0)
            {
                break;
            }

            if (layer.QuantityRemaining <= 0)
            {
                continue;
            }

            var take = Math.Min(remaining, layer.QuantityRemaining);
            var unitCost = layer.Consume(take);
            consumptions.Add(new LayerConsumption(layer.Id, take, unitCost));
            remaining -= take;
        }

        if (remaining > 0)
        {
            throw new DomainException("Insufficient inventory in cost layers for FIFO issue.");
        }

        return consumptions;
    }

    public static decimal CalculateWeightedUnitCost(IReadOnlyList<LayerConsumption> consumptions)
    {
        if (consumptions.Count == 0)
        {
            return 0m;
        }

        var totalQty = consumptions.Sum(c => c.Quantity);
        var totalCost = consumptions.Sum(c => c.Quantity * c.UnitCost);
        return totalQty == 0 ? 0m : totalCost / totalQty;
    }
}
