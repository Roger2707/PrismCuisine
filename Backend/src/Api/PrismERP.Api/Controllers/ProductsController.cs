using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.Identity.Application.Authorization;
using PrismERP.Modules.Identity.Infrastructure.Auth.Authrizations;
using PrismERP.Modules.Inventory.Application.Products;

namespace PrismERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/inventory/products")]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(PermissionCodes.ProductRead)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var products = await productService.GetAllAsync(cancellationToken);
        return Ok(products);
    }

    [HttpGet("{id}")]
    [RequirePermission(PermissionCodes.ProductRead)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet("by-sku/{sku}")]
    [RequirePermission(PermissionCodes.ProductRead)]
    public async Task<IActionResult> GetBySku(string sku, CancellationToken cancellationToken)
    {
        var product = await productService.GetBySkuAsync(sku, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.ProductWrite)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(PermissionCodes.ProductWrite)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        await productService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/deactivate")]
    [RequirePermission(PermissionCodes.ProductWrite)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        await productService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
