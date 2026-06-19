using PrismERP.BuildingBlocks.Application.Abstractions.Caching;
using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Identity.Application.Abstractions.Persistence;
using PrismERP.Modules.Identity.Application.Abstractions.Services;
using PrismERP.Modules.Identity.Domain.Entities;

namespace PrismERP.Modules.Identity.Application.Auth;

public sealed class IdentityAuthService(
    IIdentityUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtTokenProvider jwtTokenProvider,
    ICacheService cacheService) : IIdentityAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new NotFoundException("Invalid credentials.");

        if (!user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new NotFoundException("Invalid credentials.");
        }

        var roles = await unitOfWork.Authorization.GetRoleNamesByUserIdAsync(user.Id, cancellationToken);

        var (accessToken, accessTokenExpiresAt) = jwtTokenProvider.CreateAccessToken(user.Id, user.Email, roles);
        var (refreshTokenValue, refreshTokenExpiresAt) = jwtTokenProvider.CreateRefreshToken();

        user.MarkLogin();
        unitOfWork.Users.Update(user);

        var existingRefreshTokens = await unitOfWork.RefreshTokens.GetByUserIdAsync(user.Id, cancellationToken);
        if(existingRefreshTokens != null)
            existingRefreshTokens.UpdateToken(refreshTokenValue, refreshTokenExpiresAt);
        else
        {
            var refreshToken = RefreshToken.Create(user.Id, refreshTokenValue, refreshTokenExpiresAt);
            unitOfWork.RefreshTokens.Add(refreshToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new LoginResponse(accessToken, refreshTokenValue, accessTokenExpiresAt, refreshTokenExpiresAt);
    }

    public async Task LogoutAsync(int userId, LogoutRequest request, CancellationToken cancellationToken = default)
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
        int userId,
        object permissions,
        CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User was not found.");

        var roles = await unitOfWork.Authorization.GetRoleNamesByUserIdAsync(userId, cancellationToken);
        return new CurrentUserResponse(user.Id, user.Email, user.DisplayName, roles, permissions);
    }

    public async Task ChangePasswordAsync(
        int userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User was not found.");

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new BusinessException("Current password is incorrect.");
        }

        user.ChangePassword(passwordHasher.Hash(request.NewPassword));
        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ForceLogoutAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User was not found.");

        string cacheKey = $"blacklist:user:{userId}";
        await cacheService.SetAsync(cacheKey, true, TimeSpan.FromDays(30), cancellationToken);

        user.Deactivate();
        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ReleaseBlacklistAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User was not found.");

        string cacheKey = $"blacklist:user:{userId}";
        var isBlacklisted = await cacheService.ExistsAsync(cacheKey, cancellationToken);
        if(isBlacklisted)
            await cacheService.RemoveAsync(cacheKey, cancellationToken);

        user.Activate();
        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<RefreshPageResponse> RefreshPage(string refreshToken, CancellationToken cancellationToken)
    {
        var existingRefreshToken = await unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken, cancellationToken)
            ?? throw new DomainException("Refresh token is invalid.");

        if (!existingRefreshToken.IsActive())
        {
            throw new DomainException("Refresh token is invalid.");
        }

        var user = await unitOfWork.Users.GetByIdAsync(existingRefreshToken.UserId, cancellationToken)
            ?? throw new DomainException("User was not found.");

        if (!user.IsActive)
        {
            throw new DomainException("User is inactive.");
        }

        var roles = await unitOfWork.Authorization.GetRoleNamesByUserIdAsync(user.Id, cancellationToken);

        var (accessToken, accessTokenExpiresAt) = jwtTokenProvider.CreateAccessToken(user.Id, user.Email, roles);

        // F5 refresh: issue a new access token only — do not rotate refresh token here to avoid
        // RowVersion conflicts when the client fires duplicate refresh-page calls (e.g. React StrictMode).
        return new RefreshPageResponse(
            accessToken,
            refreshToken,
            accessTokenExpiresAt,
            existingRefreshToken.ExpiresAt);
    }
}