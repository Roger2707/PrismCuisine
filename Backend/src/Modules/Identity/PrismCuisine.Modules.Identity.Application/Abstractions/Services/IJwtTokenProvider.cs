namespace PrismCuisine.Modules.Identity.Application.Abstractions.Services;

public interface IJwtTokenProvider
{
    (string token, DateTime expiresAt) CreateAccessToken(Guid userId, string email, IReadOnlyCollection<string> roles);
    (string token, DateTime expiresAt) CreateRefreshToken();
}
