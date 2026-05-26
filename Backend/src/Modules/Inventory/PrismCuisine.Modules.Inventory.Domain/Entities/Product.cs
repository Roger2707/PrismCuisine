using PrismCuisine.BuildingBlocks.Domain.Aggregates;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;

namespace PrismCuisine.Modules.Inventory.Domain.Entities;

public sealed class Product : AggregateRoot
{
    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Unit { get; private set; } = null!;

    private Product()
    {
    }

    public static Product Create(string sku, string name, string unit)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new DomainException("SKU is required.");
        }

        return new Product
        {
            Sku = sku.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Unit = unit.Trim()
        };
    }
}
