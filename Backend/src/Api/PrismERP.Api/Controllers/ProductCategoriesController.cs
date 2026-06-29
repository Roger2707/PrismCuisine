using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.Identity.Application.Authorization;
using PrismERP.Modules.Identity.Infrastructure.Auth.Authrizations;
using PrismERP.Modules.Inventory.Application.ProductCategories;

namespace PrismERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/inventory/product-categories")]
public sealed class ProductCategoriesController(IProductCategoryService productCategoryService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(PermissionCodes.ProductCategoryRead)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var categories = await productCategoryService.GetAllAsync(cancellationToken);
        return Ok(categories);
    }

    [HttpGet("{id}")]
    [RequirePermission(PermissionCodes.ProductCategoryRead)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var category = await productCategoryService.GetByIdAsync(id, cancellationToken);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.ProductCategoryWrite)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await productCategoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(PermissionCodes.ProductCategoryWrite)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateProductCategoryRequest request,
        CancellationToken cancellationToken)
    {
        await productCategoryService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }
}
