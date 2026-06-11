using PrismERP.BuildingBlocks.Domain.Aggregates;
using PrismERP.BuildingBlocks.Domain.Exceptions;

namespace PrismERP.Modules.Inventory.Domain.Entities;

public sealed class ProductCategory : AggregateRoot
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private ProductCategory()
    {
    }

    public static ProductCategory Create(string code, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ValidationException("code", "Category code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("name", "Category name is required.");
        }

        return new ProductCategory
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true
        };
    }

    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("name", "Category name is required.");
        }

        Name = name.Trim();
        Description = description?.Trim();
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}
