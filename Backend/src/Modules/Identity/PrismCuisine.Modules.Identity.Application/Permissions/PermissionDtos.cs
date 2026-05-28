namespace PrismCuisine.Modules.Identity.Application.Permissions;

public sealed record PermissionDto(Guid Id, string Code, string Description);
public sealed record CreatePermissionRequest(string Code, string Description);
public sealed record UpdatePermissionRequest(Guid Id, string Code, string Description);

