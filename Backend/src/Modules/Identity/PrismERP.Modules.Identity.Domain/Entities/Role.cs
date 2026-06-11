using PrismERP.BuildingBlocks.Domain.Aggregates;
using PrismERP.BuildingBlocks.Domain.Exceptions;

namespace PrismERP.Modules.Identity.Domain.Entities;

public sealed class Role : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public string NormalizedName { get; private set; } = null!;

    private Role()
    {
    }

    public static Role Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("name", "Role name is required.");
        }

        return new Role
        {
            Name = name.Trim(),
            NormalizedName = name.Trim().ToUpperInvariant()
        };
    }
}
