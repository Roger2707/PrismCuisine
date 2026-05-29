using PrismCuisine.BuildingBlocks.Domain.Aggregates;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;

namespace PrismCuisine.Modules.Inventory.Domain.Entities;

public sealed class Warehouse : AggregateRoot
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Location { get; private set; }
    public bool IsActive { get; private set; }

    private Warehouse()
    {
    }

    public static Warehouse Create(string code, string name, string? location = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Warehouse code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Warehouse name is required.");
        }

        return new Warehouse
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Location = location?.Trim(),
            IsActive = true
        };
    }

    public void Update(string name, string? location)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Warehouse name is required.");
        }

        Name = name.Trim();
        Location = location?.Trim();
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}
