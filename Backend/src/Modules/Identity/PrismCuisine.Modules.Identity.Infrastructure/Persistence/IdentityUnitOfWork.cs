using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Identity.Application.Abstractions.Persistence;
using PrismCuisine.Modules.Identity.Infrastructure.Persistence.Repositories;

namespace PrismCuisine.Modules.Identity.Infrastructure.Persistence;

internal sealed class IdentityUnitOfWork(PrismCuisineDbContext db) : IIdentityUnitOfWork
{
    public IUserRepository Users { get; } = new UserRepository(db);
    public IRefreshTokenRepository RefreshTokens { get; } = new RefreshTokenRepository(db);
    public IIdentityAuthorizationRepository Authorization { get; } = new IdentityAuthorizationRepository(db);
    public IPermissionRepository Permission { get; } = new PermissionRepository(db);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
