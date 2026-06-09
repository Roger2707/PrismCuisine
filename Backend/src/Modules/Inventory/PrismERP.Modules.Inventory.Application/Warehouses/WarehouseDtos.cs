namespace PrismERP.Modules.Inventory.Application.Warehouses;

public sealed record WarehouseDto(
    int Id,
    string Code,
    string Name,
    string? Location,
    bool IsActive);

public sealed record CreateWarehouseRequest(string Code, string Name, string? Location);

public sealed record UpdateWarehouseRequest(string Name, string? Location);
