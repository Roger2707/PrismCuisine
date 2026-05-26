using PrismCuisine.BuildingBlocks.Application.Abstractions.Caching;
using PrismCuisine.BuildingBlocks.Application.Abstractions.Cqrs;
using PrismCuisine.Modules.Identity.Application.Abstractions.Persistence;

namespace PrismCuisine.Modules.Identity.Application.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler(
    IUserRepository users,
    IIdentityAuthorizationRepository authorization,
    ICacheService cache) : IQueryHandler<GetUserByIdQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"identity:user:{request.UserId}";
        var cached = await cache.GetAsync<UserDto>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var user = await users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var roles = await authorization.GetRoleNamesByUserIdAsync(request.UserId, cancellationToken);
        var dto = new UserDto(user.Id, user.Email, user.DisplayName, user.IsActive, roles);

        await cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5), cancellationToken);
        return dto;
    }
}