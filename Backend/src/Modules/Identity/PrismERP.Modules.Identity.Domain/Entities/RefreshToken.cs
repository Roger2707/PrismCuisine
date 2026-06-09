using PrismERP.BuildingBlocks.Domain.Aggregates;

namespace PrismERP.Modules.Identity.Domain.Entities;

public sealed class RefreshToken : AggregateRoot
{
    public int UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    private RefreshToken()
    {
    }

    public static RefreshToken Create(int userId, string token, DateTime expiresAt) =>
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

    public void UpdateToken(string token, DateTime expiresAt)
    {
        Token = token;
        ExpiresAt = expiresAt;
        Touch();
    }
}
