using PrismCuisine.BuildingBlocks.Domain.Aggregates;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;

namespace PrismCuisine.Modules.Identity.Domain.Entities;

public sealed class Permission : AggregateRoot
{
    public string Code { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    private Permission()
    {
    }

    public static Permission Create(string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Permission code is required.");
        }

        return new Permission
        {
            Code = code.Trim().ToLowerInvariant(),
            Description = description.Trim()
        };
    }

    public void Update(string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Permission code is required.");
        }

        Code = code.Trim().ToLowerInvariant();
        Description = description.Trim();
    }

    public void Delete()
    {
        MarkDeleted();
    }
}
