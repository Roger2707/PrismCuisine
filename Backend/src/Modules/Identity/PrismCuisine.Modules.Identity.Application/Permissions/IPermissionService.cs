using PrismCuisine.Modules.Identity.Domain.Entities;

namespace PrismCuisine.Modules.Identity.Application.Permissions
{
    public interface IPermissionService
    {
        Task<IReadOnlyCollection<Permission>> GetPermissionsReadOnlyAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<Permission>> GetPermissionsReadOnlyByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task<Permission?> GetPermissionByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<Permission?> GetPermissionReadOnlyByCodeAsync(string code, CancellationToken cancellationToken = default);
        void Add(CreatePermissionRequest permission);
        Task Update(UpdatePermissionRequest permission);
        Task Delete(string permissionCode);
    }
}
