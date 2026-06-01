using PrismCuisine.BuildingBlocks.Domain.Aggregates;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;

namespace PrismCuisine.Modules.Purchasing.Domain.Entities;

public sealed class Supplier : AggregateRoot
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? TaxCode { get; private set; }
    public bool IsActive { get; private set; }

    private Supplier()
    {
    }

    public static Supplier Create(
        string code,
        string name,
        string? phone = null,
        string? email = null,
        string? address = null,
        string? taxCode = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Supplier code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Supplier name is required.");
        }

        return new Supplier
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Phone = phone?.Trim(),
            Email = email?.Trim(),
            Address = address?.Trim(),
            TaxCode = taxCode?.Trim(),
            IsActive = true
        };
    }

    public void Update(
        string name,
        string? phone,
        string? email,
        string? address,
        string? taxCode)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Supplier name is required.");
        }

        Name = name.Trim();
        Phone = phone?.Trim();
        Email = email?.Trim();
        Address = address?.Trim();
        TaxCode = taxCode?.Trim();
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}
