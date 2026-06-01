namespace PrismCuisine.Modules.Identity.Application.Permissions;

public sealed record PermissionDto(int Id, string Code, string Description);
public sealed record CreatePermissionRequest(string Code, string Description);
public sealed record UpdatePermissionRequest(int Id, string Code, string Description);

