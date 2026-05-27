using PrismCuisine.Modules.Identity.Application.Abstractions.Persistence;

namespace PrismCuisine.Modules.Identity.Application.Users;

public sealed class UserService(
    IUserRepository users,
    IIdentityAuthorizationRepository authorization) : IUserService
{
    public async Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var roles = await authorization.GetRoleNamesByUserIdAsync(userId, cancellationToken);
        var dto = new UserDto(user.Id, user.Email, user.DisplayName, user.IsActive, roles);
        return dto;
    }
}
