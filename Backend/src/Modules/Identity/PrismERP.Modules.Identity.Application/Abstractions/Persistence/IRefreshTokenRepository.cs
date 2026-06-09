using PrismERP.Modules.Identity.Domain.Entities;

namespace PrismERP.Modules.Identity.Application.Abstractions.Persistence;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    void Add(RefreshToken refreshToken);
    void Update(RefreshToken refreshToken);
}
