namespace PrismCuisine.Modules.Identity.Application.Auth;

public interface IIdentityAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(Guid userId, LogoutRequest request, CancellationToken cancellationToken = default);
    Task<CurrentUserResponse> GetCurrentUserAsync(Guid userId, object permissions, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task ForceLogoutAsync(Guid userId, CancellationToken cancellationToken = default);
    Task ReleaseBlacklistAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<RefreshPageResponse> RefreshPage(string refreshToken, CancellationToken cancellationToken);
}
