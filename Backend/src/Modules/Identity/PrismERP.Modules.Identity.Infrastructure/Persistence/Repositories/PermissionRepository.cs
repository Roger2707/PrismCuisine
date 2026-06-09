using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Identity.Application.Abstractions.Persistence;
using PrismERP.Modules.Identity.Domain.Entities;

namespace PrismERP.Modules.Identity.Infrastructure.Persistence.Repositories
{
    public class PermissionRepository(PrismERPDbContext db) : IPermissionRepository
    {
        public async Task<IReadOnlyCollection<Permission>> GetPermissionsReadOnlyAsync(CancellationToken cancellationToken = default)
        {
            var permissions = await db.Permissions.AsNoTracking().ToListAsync(cancellationToken);
            return permissions;
        }

        public Task<Permission?> GetPermissionByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            var permission = db.Permissions.FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
            return permission;
        }

        public Task<Permission?> GetPermissionReadOnlyByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            var permission = db.Permissions.AsNoTracking().FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
            return permission;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(int roleId, CancellationToken cancellationToken = default)
        {
            var permissionIds = await db.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync(cancellationToken);

            var permissions = await db.Permissions.Where(p => permissionIds.Contains(p.Id)).ToListAsync(cancellationToken);
            return permissions;
        }

        public async Task<IReadOnlyCollection<Permission>> GetPermissionsReadOnlyByRoleAsync(int roleId, CancellationToken cancellationToken = default)
        {
            var permissionIds = await db.RolePermissions
                                    .Where(rp => rp.RoleId == roleId)
                                    .AsNoTracking()
                                    .Select(rp => rp.PermissionId)
                                    .ToListAsync(cancellationToken);

            var permissions = await db.Permissions.Where(p => permissionIds.Contains(p.Id)).ToListAsync(cancellationToken);
            return permissions;
        }

        public void Add(Permission permission)
        {
            db.Permissions.Add(permission);
        }

        public void Update(Permission permission)
        {
            db.Permissions.Update(permission);
        }
    }
}
