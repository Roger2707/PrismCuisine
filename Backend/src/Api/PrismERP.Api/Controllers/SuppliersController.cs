using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.Identity.Application.Authorization;
using PrismERP.Modules.Identity.Infrastructure.Auth.Authrizations;
using PrismERP.Modules.Purchasing.Application.Suppliers;

namespace PrismERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/purchasing/suppliers")]
public sealed class SuppliersController(ISupplierService supplierService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(PermissionCodes.SupplierRead)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var suppliers = await supplierService.GetAllAsync(cancellationToken);
        return Ok(suppliers);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(PermissionCodes.SupplierRead)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var supplier = await supplierService.GetByIdAsync(id, cancellationToken);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.SupplierWrite)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var supplier = await supplierService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, supplier);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(PermissionCodes.SupplierWrite)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        await supplierService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/deactivate")]
    [RequirePermission(PermissionCodes.SupplierWrite)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        await supplierService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
