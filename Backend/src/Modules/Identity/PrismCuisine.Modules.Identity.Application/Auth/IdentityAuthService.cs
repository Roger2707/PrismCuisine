using PrismCuisine.BuildingBlocks.Application.Abstractions.Caching;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.Identity.Application.Abstractions.Persistence;
using PrismCuisine.Modules.Identity.Application.Abstractions.Services;
using PrismCuisine.Modules.Identity.Domain.Entities;

namespace PrismCuisine.Modules.Identity.Application.Auth;

public sealed class IdentityAuthService(
    IIdentityUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtTokenProvider jwtTokenProvider,
    ICacheService cacheService) : IIdentityAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new DomainException("Invalid credentials.");

        if (!user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new DomainException("Invalid credentials.");
        }

        var roles = await unitOfWork.Authorization.GetRoleNamesByUserIdAsync(user.Id, cancellationToken);

        var (accessToken, accessTokenExpiresAt) = jwtTokenProvider.CreateAccessToken(user.Id, user.Email, roles);
        var (refreshTokenValue, refreshTokenExpiresAt) = jwtTokenProvider.CreateRefreshToken();

        user.MarkLogin();
        var refreshToken = RefreshToken.Create(user.Id, refreshTokenValue, refreshTokenExpiresAt);

        unitOfWork.Users.Update(user);
        unitOfWork.RefreshTokens.Add(refreshToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse(accessToken, refreshTokenValue, accessTokenExpiresAt, refreshTokenExpiresAt);
    }

    public async Task LogoutAsync(Guid userId, LogoutRequest request, CancellationToken cancellationToken = default)
    {
        var refreshToken = await unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken)
            ?? throw new DomainException("Refresh token is invalid.");

        if (refreshToken.UserId != userId || !refreshToken.IsActive())
        {
            throw new DomainException("Refresh token is invalid.");
        }

        refreshToken.Revoke();
        unitOfWork.RefreshTokens.Update(refreshToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync(
        Guid userId,
        object permissions,
        CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new DomainException("User was not found.");

        var roles = await unitOfWork.Authorization.GetRoleNamesByUserIdAsync(userId, cancellationToken);
        return new CurrentUserResponse(user.Id, user.Email, user.DisplayName, roles, permissions);
    }

    public async Task ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new DomainException("User was not found.");

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new DomainException("Current password is incorrect.");
        }

        user.ChangePassword(passwordHasher.Hash(request.NewPassword));
        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ForceLogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new DomainException("User was not found.");

        string cacheKey = $"blacklist:user:{userId}";
        await cacheService.SetAsync(cacheKey, true, TimeSpan.FromDays(30), cancellationToken);

        user.Deactivate();
        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ReleaseBlacklistAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new DomainException("User was not found.");

        string cacheKey = $"blacklist:user:{userId}";
        var isBlacklisted = await cacheService.ExistsAsync(cacheKey, cancellationToken);
        if(isBlacklisted)
            await cacheService.RemoveAsync(cacheKey, cancellationToken);

        user.Activate();
        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
