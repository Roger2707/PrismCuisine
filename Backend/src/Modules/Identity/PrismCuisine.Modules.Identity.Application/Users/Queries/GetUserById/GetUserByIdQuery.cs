using PrismCuisine.BuildingBlocks.Application.Abstractions.Cqrs;

namespace PrismCuisine.Modules.Identity.Application.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserDto?>;

public sealed record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    bool IsActive,
    IReadOnlyCollection<string> Roles);
