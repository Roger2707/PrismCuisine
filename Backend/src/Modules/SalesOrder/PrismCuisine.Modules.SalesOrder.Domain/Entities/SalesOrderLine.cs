using PrismCuisine.BuildingBlocks.Domain.Entities;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;

namespace PrismCuisine.Modules.SalesOrder.Domain.Entities;

public sealed class SalesOrderLine : Entity
{
    public int SalesOrderId { get; private set; }
    public int ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private SalesOrderLine()
    {
    }

    internal static SalesOrderLine Create(int productId, decimal quantity, decimal unitPrice)
    {
        if (productId <= 0)
        {
            throw new DomainException("ProductId is required.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }

        return new SalesOrderLine
        {
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }

    internal void AssignToOrder(int salesOrderId) => SalesOrderId = salesOrderId;
}
