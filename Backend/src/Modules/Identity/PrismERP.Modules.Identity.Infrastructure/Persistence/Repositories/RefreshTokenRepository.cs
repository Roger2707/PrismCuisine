using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Identity.Application.Abstractions.Persistence;
using PrismERP.Modules.Identity.Domain.Entities;

namespace PrismERP.Modules.Identity.Infrastructure.Persistence.Repositories;

internal sealed class RefreshTokenRepository(PrismERPDbContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenAsync(string refreshToken, CancellationToken cancellationToken = default) =>
        db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);

    public void Add(RefreshToken refreshToken) => db.RefreshTokens.Add(refreshToken);

    public void Update(RefreshToken refreshToken) => db.RefreshTokens.Update(refreshToken);

    public Task<RefreshToken?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return db.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }
}
