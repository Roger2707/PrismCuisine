using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Identity.Application.Abstractions.Persistence;

namespace PrismCuisine.Modules.Identity.Infrastructure.Persistence.Repositories;

internal sealed class IdentityAuthorizationRepository(PrismCuisineDbContext db) : IIdentityAuthorizationRepository
{
    public async Task<IReadOnlyCollection<string>> GetRoleNamesByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var roleIds = await db.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        if (roleIds.Count == 0)
        {
            return [];
        }

        var roles = await db.Roles
            .AsNoTracking()
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync(cancellationToken);

        return roles.Select(r => r.Name).ToList();
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionCodesByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var roleIds = await db.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        if (roleIds.Count == 0)
        {
            return [];
        }

        var permissionIds = await db.RolePermissions
            .AsNoTracking()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.PermissionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (permissionIds.Count == 0)
        {
            return [];
        }

        var permissions = await db.Permissions
            .AsNoTracking()
            .Where(p => permissionIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        return permissions.Select(p => p.Code).ToList();
    }

    public async Task<bool> IsSuperAdminAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var roleIds = await db.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        if (roleIds.Count == 0)
        {
            return false;
        }

        return await db.Roles
            .AsNoTracking()
            .AnyAsync(r => roleIds.Contains(r.Id) && r.NormalizedName == "SUPER_ADMIN", cancellationToken);
    }
}
