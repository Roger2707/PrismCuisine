using PrismCuisine.BuildingBlocks.Domain.Entities;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;

namespace PrismCuisine.Modules.Purchasing.Domain.Entities;

public sealed class PurchaseOrderLine : Entity
{
    public int PurchaseOrderId { get; private set; }
    public int ProductId { get; private set; }
    public decimal QuantityOrdered { get; private set; }
    public decimal QuantityReceived { get; private set; }
    public decimal UnitPrice { get; private set; }

    private PurchaseOrderLine()
    {
    }

    public decimal QuantityRemaining => QuantityOrdered - QuantityReceived;

    internal static PurchaseOrderLine Create(int productId, decimal quantityOrdered, decimal unitPrice)
    {
        if (productId <= 0)
        {
            throw new DomainException("ProductId is required.");
        }

        if (quantityOrdered <= 0)
        {
            throw new DomainException("Ordered quantity must be greater than zero.");
        }

        if (unitPrice < 0)
        {
            throw new DomainException("Unit price cannot be negative.");
        }

        return new PurchaseOrderLine
        {
            ProductId = productId,
            QuantityOrdered = quantityOrdered,
            QuantityReceived = 0m,
            UnitPrice = unitPrice
        };
    }

    internal void AssignToOrder(int purchaseOrderId) => PurchaseOrderId = purchaseOrderId;

    internal void RecordReceipt(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Receipt quantity must be greater than zero.");
        }

        if (quantity > QuantityRemaining)
        {
            throw new DomainException("Receipt quantity exceeds remaining ordered quantity.");
        }

        QuantityReceived += quantity;
    }

    internal bool IsFullyReceived() => QuantityReceived >= QuantityOrdered;
}
