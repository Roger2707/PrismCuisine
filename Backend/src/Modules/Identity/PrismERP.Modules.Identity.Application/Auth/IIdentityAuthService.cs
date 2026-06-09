namespace PrismERP.Modules.Identity.Application.Auth;

public interface IIdentityAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(int userId, LogoutRequest request, CancellationToken cancellationToken = default);
    Task<CurrentUserResponse> GetCurrentUserAsync(int userId, object permissions, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task ForceLogoutAsync(int userId, CancellationToken cancellationToken = default);
    Task ReleaseBlacklistAsync(int userId, CancellationToken cancellationToken = default);
    Task<RefreshPageResponse> RefreshPage(string refreshToken, CancellationToken cancellationToken);
}
