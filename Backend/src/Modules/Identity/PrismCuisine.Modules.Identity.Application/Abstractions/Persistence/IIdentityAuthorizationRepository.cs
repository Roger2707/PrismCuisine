namespace PrismCuisine.Modules.Identity.Application.Abstractions.Persistence;

public interface IIdentityAuthorizationRepository
{
    Task<IReadOnlyCollection<string>> GetRoleNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetPermissionCodesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsSuperAdminAsync(Guid userId, CancellationToken cancellationToken = default);
}
