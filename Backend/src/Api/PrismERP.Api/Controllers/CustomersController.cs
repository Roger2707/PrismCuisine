using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.SalesOrdering.Application.Customers;

namespace PrismERP.Api.Controllers;

[ApiController]
[Route("api/sales-ordering/customers")]
public sealed class CustomersController(ICustomerService customerService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var customers = await customerService.GetAllAsync(cancellationToken);
        return Ok(customers);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var customer = await customerService.GetByIdAsync(id, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await customerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        await customerService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        await customerService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
