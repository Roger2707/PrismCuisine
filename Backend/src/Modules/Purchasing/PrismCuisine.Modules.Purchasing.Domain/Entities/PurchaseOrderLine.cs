using PrismCuisine.BuildingBlocks.Domain.Entities;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;

namespace PrismCuisine.Modules.Purchasing.Domain.Entities;

public sealed class PurchaseOrderLine : Entity
{
    public Guid PurchaseOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private PurchaseOrderLine()
    {
    }

    internal static PurchaseOrderLine Create(Guid productId, decimal quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
        {
            throw new DomainException("ProductId is required.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }

        if (unitPrice < 0)
        {
            throw new DomainException("Unit price cannot be negative.");
        }

        return new PurchaseOrderLine
        {
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }

    internal void AssignToOrder(Guid purchaseOrderId) => PurchaseOrderId = purchaseOrderId;
}
