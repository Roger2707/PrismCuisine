using PrismCuisine.BuildingBlocks.Application.Abstractions.Caching;
using PrismCuisine.Modules.Identity.Application.Abstractions.Persistence;

namespace PrismCuisine.Modules.Identity.Application.Users;

public sealed class UserService(
    IUserRepository users,
    IIdentityAuthorizationRepository authorization,
    ICacheService cache) : IUserService
{
    public async Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"identity:user:{userId}";
        var cached = await cache.GetAsync<UserDto>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var roles = await authorization.GetRoleNamesByUserIdAsync(userId, cancellationToken);
        var dto = new UserDto(user.Id, user.Email, user.DisplayName, user.IsActive, roles);

        await cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5), cancellationToken);
        return dto;
    }
}
