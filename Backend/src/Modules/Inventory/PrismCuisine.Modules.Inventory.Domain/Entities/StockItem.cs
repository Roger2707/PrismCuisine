using PrismCuisine.BuildingBlocks.Domain.Aggregates;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;

namespace PrismCuisine.Modules.Inventory.Domain.Entities;

public sealed class StockItem : AggregateRoot
{
    public Guid ProductId { get; private set; }
    public decimal QuantityOnHand { get; private set; }
    public decimal ReorderLevel { get; private set; }

    private StockItem()
    {
    }

    public static StockItem Create(Guid productId, decimal initialQuantity, decimal reorderLevel)
    {
        if (productId == Guid.Empty)
        {
            throw new DomainException("ProductId is required.");
        }

        if (initialQuantity < 0)
        {
            throw new DomainException("Initial quantity cannot be negative.");
        }

        return new StockItem
        {
            ProductId = productId,
            QuantityOnHand = initialQuantity,
            ReorderLevel = reorderLevel
        };
    }

    public void Adjust(decimal delta)
    {
        var newQty = QuantityOnHand + delta;
        if (newQty < 0)
        {
            throw new DomainException("Insufficient stock.");
        }

        QuantityOnHand = newQty;
        Touch();
    }
}
