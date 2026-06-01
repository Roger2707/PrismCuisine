namespace PrismCuisine.Modules.Identity.Application.Abstractions.Persistence;

public interface IIdentityAuthorizationRepository
{
    Task<IReadOnlyCollection<string>> GetRoleNamesByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetPermissionCodesByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> IsSuperAdminAsync(int userId, CancellationToken cancellationToken = default);
}
