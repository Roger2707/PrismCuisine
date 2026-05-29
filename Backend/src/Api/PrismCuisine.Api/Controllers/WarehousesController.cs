using Microsoft.AspNetCore.Mvc;
using PrismCuisine.Modules.Inventory.Application.Warehouses;

namespace PrismCuisine.Api.Controllers;

[ApiController]
[Route("api/inventory/warehouses")]
public sealed class WarehousesController(IWarehouseService warehouseService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var warehouses = await warehouseService.GetAllAsync(cancellationToken);
        return Ok(warehouses);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var warehouse = await warehouseService.GetByIdAsync(id, cancellationToken);
        return warehouse is null ? NotFound() : Ok(warehouse);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateWarehouseRequest request,
        CancellationToken cancellationToken)
    {
        var warehouse = await warehouseService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = warehouse.Id }, warehouse);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateWarehouseRequest request,
        CancellationToken cancellationToken)
    {
        await warehouseService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }
}
