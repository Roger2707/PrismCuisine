using PrismCuisine.Modules.Identity.Application.Abstractions.Persistence;
using PrismCuisine.Modules.Identity.Domain.Entities;
using System.Threading.Tasks;

namespace PrismCuisine.Modules.Identity.Application.Permissions
{
    public sealed class PermissionService(IIdentityUnitOfWork unitOfWork) : IPermissionService
    {
        #region ReadOnly Methods

        public async Task<IReadOnlyCollection<Permission>> GetPermissionsReadOnlyAsync(CancellationToken cancellationToken = default)
        {
            var permissions = await unitOfWork.Permission.GetPermissionsReadOnlyAsync(cancellationToken);
            return permissions;
        }

        public async Task<IReadOnlyCollection<Permission>> GetPermissionsReadOnlyByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            var permissions = await unitOfWork.Permission.GetPermissionsReadOnlyByRoleAsync(roleId, cancellationToken);
            return permissions;
        }

        public Task<Permission?> GetPermissionReadOnlyByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            var permission = unitOfWork.Permission.GetPermissionReadOnlyByCodeAsync(code, cancellationToken);
            return permission;
        }

        #endregion

        public async Task<Permission?> GetPermissionByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            var permission = await unitOfWork.Permission.GetPermissionByCodeAsync(code, cancellationToken);
            return permission;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            var permissions = await unitOfWork.Permission.GetPermissionsByRoleAsync(roleId, cancellationToken);
            return permissions;
        }

        public async Task Add(CreatePermissionRequest permission)
        {
            var newPermission = Permission.Create(permission.Code, permission.Description);
            unitOfWork.Permission.Add(newPermission);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task Update(UpdatePermissionRequest permission)
        {
            var existingPermission = await unitOfWork.Permission.GetPermissionByCodeAsync(permission.Code);
            if(existingPermission is null)
                throw new KeyNotFoundException($"Permission with code '{permission.Code}' not found.");

            existingPermission.Update(permission.Code, permission.Description);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task Delete(string permissionCode)
        {
            var permission = await unitOfWork.Permission.GetPermissionByCodeAsync(permissionCode);
            if (permission is null)
                throw new KeyNotFoundException($"Permission with code '{permissionCode}' not found.");
            permission.Delete();
            await unitOfWork.SaveChangesAsync();
        }
    }
}
