using PrismERP.BuildingBlocks.Domain.Entities;

namespace PrismERP.Modules.Identity.Domain.Entities;

public sealed class UserRole : Entity
{
    public int UserId { get; private set; }
    public int RoleId { get; private set; }

    private UserRole()
    {
    }

    public static UserRole Create(int userId, int roleId) =>
        new()
        {
            UserId = userId,
            RoleId = roleId
        };
}
