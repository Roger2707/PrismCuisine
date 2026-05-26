using PrismCuisine.BuildingBlocks.Domain.Entities;

namespace PrismCuisine.Modules.Identity.Domain.Entities;

public sealed class UserRole : Entity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    private UserRole()
    {
    }

    public static UserRole Create(Guid userId, Guid roleId) =>
        new()
        {
            UserId = userId,
            RoleId = roleId
        };
}
