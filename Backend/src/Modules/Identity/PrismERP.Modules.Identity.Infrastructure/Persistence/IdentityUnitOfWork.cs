using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Identity.Application.Abstractions.Persistence;
using PrismERP.Modules.Identity.Infrastructure.Persistence.Repositories;

namespace PrismERP.Modules.Identity.Infrastructure.Persistence;

internal sealed class IdentityUnitOfWork(PrismERPDbContext db) : IIdentityUnitOfWork
{
    public IUserRepository Users { get; } = new UserRepository(db);
    public IRefreshTokenRepository RefreshTokens { get; } = new RefreshTokenRepository(db);
    public IIdentityAuthorizationRepository Authorization { get; } = new IdentityAuthorizationRepository(db);
    public IPermissionRepository Permission { get; } = new PermissionRepository(db);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
