namespace PrismCuisine.Modules.Identity.Application.Users;

public sealed record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    bool IsActive,
    IReadOnlyCollection<string> Roles);
