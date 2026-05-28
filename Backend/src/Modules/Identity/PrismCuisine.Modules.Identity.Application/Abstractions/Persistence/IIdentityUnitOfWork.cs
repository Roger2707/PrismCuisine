using PrismCuisine.BuildingBlocks.Application.Abstractions.Persistence;

namespace PrismCuisine.Modules.Identity.Application.Abstractions.Persistence;

public interface IIdentityUnitOfWork : IUnitOfWork
{
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IIdentityAuthorizationRepository Authorization { get; }
    IPermissionRepository Permission { get; }
}
