namespace PrismCuisine.Modules.Identity.Application.Users;

public sealed record UserDto(
    int Id,
    string Email,
    string DisplayName,
    bool IsActive,
    IReadOnlyCollection<string> Roles);
