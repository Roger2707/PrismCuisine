using PrismCuisine.BuildingBlocks.Domain.Aggregates;

namespace PrismCuisine.Modules.SalesOrdering.Domain.Entities;

public sealed class DeliveryNoteLine : AggregateRoot
{
    private DeliveryNoteLine() { } // EF Core

    public int DeliveryNoteId { get; private set; }

    // Reference về SalesOrderLine để track
    public int SalesOrderLineId { get; private set; }

    // Snapshot
    public int ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public decimal QuantityDelivered { get; private set; }

    internal static DeliveryNoteLine Create(int salesOrderLineId, int productId, string productName, decimal quantityDelivered)
    {
        return new DeliveryNoteLine
        {
            SalesOrderLineId = salesOrderLineId,
            ProductId = productId,
            ProductName = productName, // snapshot
            QuantityDelivered = quantityDelivered
        };
    }
}
