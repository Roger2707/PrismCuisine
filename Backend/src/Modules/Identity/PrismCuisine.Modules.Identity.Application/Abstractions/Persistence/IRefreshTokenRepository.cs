using PrismCuisine.Modules.Identity.Domain.Entities;

namespace PrismCuisine.Modules.Identity.Application.Abstractions.Persistence;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    void Add(RefreshToken refreshToken);
    void Update(RefreshToken refreshToken);
}
