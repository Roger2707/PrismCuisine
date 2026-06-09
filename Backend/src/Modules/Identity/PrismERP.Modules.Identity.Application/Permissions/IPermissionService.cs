using PrismERP.Modules.Identity.Domain.Entities;

namespace PrismERP.Modules.Identity.Application.Permissions
{
    public interface IPermissionService
    {
        Task<IReadOnlyCollection<Permission>> GetPermissionsReadOnlyAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(int roleId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<Permission>> GetPermissionsReadOnlyByRoleAsync(int roleId, CancellationToken cancellationToken = default);
        Task<Permission?> GetPermissionByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<Permission?> GetPermissionReadOnlyByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task Add(CreatePermissionRequest permission);
        Task Update(UpdatePermissionRequest permission);
        Task Delete(string permissionCode);
    }
}
