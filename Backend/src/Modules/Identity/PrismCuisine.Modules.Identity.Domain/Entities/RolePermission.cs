using PrismCuisine.BuildingBlocks.Domain.Entities;

namespace PrismCuisine.Modules.Identity.Domain.Entities;

public sealed class RolePermission : Entity
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }

    private RolePermission()
    {
    }

    public static RolePermission Create(Guid roleId, Guid permissionId) =>
        new()
        {
            RoleId = roleId,
            PermissionId = permissionId
        };
}
