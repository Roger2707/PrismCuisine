using PrismCuisine.BuildingBlocks.Domain.Aggregates;

namespace PrismCuisine.Modules.Identity.Domain.Entities;

public sealed class RefreshToken : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    private RefreshToken()
    {
    }

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt) =>
        new()
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt
        };

    public bool IsActive() => RevokedAt is null && ExpiresAt > DateTime.UtcNow;

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
        Touch();
    }
}
