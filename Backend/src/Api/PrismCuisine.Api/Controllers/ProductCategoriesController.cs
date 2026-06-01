using Microsoft.AspNetCore.Mvc;
using PrismCuisine.Modules.Inventory.Application.ProductCategories;

namespace PrismCuisine.Api.Controllers;

[ApiController]
[Route("api/inventory/product-categories")]
public sealed class ProductCategoriesController(IProductCategoryService productCategoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var categories = await productCategoryService.GetAllAsync(cancellationToken);
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var category = await productCategoryService.GetByIdAsync(id, cancellationToken);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await productCategoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateProductCategoryRequest request,
        CancellationToken cancellationToken)
    {
        await productCategoryService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }
}
