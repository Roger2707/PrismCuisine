using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.Purchasing.Application.Suppliers;

namespace PrismERP.Api.Controllers;

[ApiController]
[Route("api/purchasing/suppliers")]
public sealed class SuppliersController(ISupplierService supplierService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var suppliers = await supplierService.GetAllAsync(cancellationToken);
        return Ok(suppliers);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var supplier = await supplierService.GetByIdAsync(id, cancellationToken);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var supplier = await supplierService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, supplier);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        await supplierService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        await supplierService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
