namespace PrismCuisine.Modules.Identity.Application.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt);

public sealed record LogoutRequest(string RefreshToken);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed record CurrentUserResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    IReadOnlyCollection<string> Roles,
    object Permissions);
