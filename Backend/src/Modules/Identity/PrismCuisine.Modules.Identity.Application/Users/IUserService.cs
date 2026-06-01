namespace PrismCuisine.Modules.Identity.Application.Users;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);
}
