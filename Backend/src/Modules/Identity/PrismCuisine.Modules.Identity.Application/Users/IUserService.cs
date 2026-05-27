namespace PrismCuisine.Modules.Identity.Application.Users;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
