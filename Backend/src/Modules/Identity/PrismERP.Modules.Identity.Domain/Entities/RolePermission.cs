using PrismERP.BuildingBlocks.Domain.Entities;

namespace PrismERP.Modules.Identity.Domain.Entities;

public sealed class RolePermission : Entity
{
    public int RoleId { get; private set; }
    public int PermissionId { get; private set; }

    private RolePermission()
    {
    }

    public static RolePermission Create(int roleId, int permissionId) =>
        new()
        {
            RoleId = roleId,
            PermissionId = permissionId
        };

    public void Update(int roleId, int permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
        Touch();
    }
}
