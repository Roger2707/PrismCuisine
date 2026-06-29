using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.Identity.Application.Authorization;
using PrismERP.Modules.Identity.Infrastructure.Auth.Authrizations;
using PrismERP.Modules.Inventory.Application.Warehouses;

namespace PrismERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/inventory/warehouses")]
public sealed class WarehousesController(IWarehouseService warehouseService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(PermissionCodes.WarehouseRead)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var warehouses = await warehouseService.GetAllAsync(cancellationToken);
        return Ok(warehouses);
    }

    [HttpGet("{id}")]
    [RequirePermission(PermissionCodes.WarehouseRead)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var warehouse = await warehouseService.GetByIdAsync(id, cancellationToken);
        return warehouse is null ? NotFound() : Ok(warehouse);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.WarehouseWrite)]
    public async Task<IActionResult> Create(
        [FromBody] CreateWarehouseRequest request,
        CancellationToken cancellationToken)
    {
        var warehouse = await warehouseService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = warehouse.Id }, warehouse);
    }

    [HttpPut("{id}")]
    [RequirePermission(PermissionCodes.WarehouseWrite)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateWarehouseRequest request,
        CancellationToken cancellationToken)
    {
        await warehouseService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }
}
