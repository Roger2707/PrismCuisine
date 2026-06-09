using PrismERP.BuildingBlocks.Application.Abstractions.Persistence;

namespace PrismERP.Modules.Identity.Application.Abstractions.Persistence;

public interface IIdentityUnitOfWork : IUnitOfWork
{
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IIdentityAuthorizationRepository Authorization { get; }
    IPermissionRepository Permission { get; }
}
