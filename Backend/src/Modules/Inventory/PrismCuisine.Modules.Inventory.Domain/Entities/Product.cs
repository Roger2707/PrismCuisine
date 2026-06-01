using PrismCuisine.BuildingBlocks.Domain.Aggregates;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;

namespace PrismCuisine.Modules.Inventory.Domain.Entities;

public sealed class Product : AggregateRoot
{
    public int CategoryId { get; private set; }
    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Unit { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private Product()
    {
    }

    public static Product Create(
        int categoryId,
        string sku,
        string name,
        string unit,
        string? description = null)
    {
        if (categoryId <= 0)
        {
            throw new DomainException("CategoryId is required.");
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new DomainException("SKU is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name is required.");
        }

        return new Product
        {
            CategoryId = categoryId,
            Sku = sku.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Unit = unit.Trim(),
            Description = description?.Trim(),
            IsActive = true
        };
    }

    public void Update(int categoryId, string name, string unit, string? description)
    {
        if (categoryId <= 0)
        {
            throw new DomainException("CategoryId is required.");
        }

        CategoryId = categoryId;
        Name = name.Trim();
        Unit = unit.Trim();
        Description = description?.Trim();
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}
