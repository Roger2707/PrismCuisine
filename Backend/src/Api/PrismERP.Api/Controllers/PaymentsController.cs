using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.Finance.Application.Payments;

namespace PrismERP.Api.Controllers;

[ApiController]
[Route("api/finance/payments")]
public sealed class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var payments = await paymentService.GetAllAsync(cancellationToken);
        return Ok(payments);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var payment = await paymentService.GetByIdAsync(id, cancellationToken);
        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpGet("by-invoice/{invoiceId:int}")]
    public async Task<IActionResult> GetByInvoice(int invoiceId, CancellationToken cancellationToken)
    {
        var payments = await paymentService.GetByInvoiceIdAsync(invoiceId, cancellationToken);
        return Ok(payments);
    }

    [HttpGet("generate-number")]
    public async Task<IActionResult> GenerateNumber(CancellationToken cancellationToken)
    {
        var number = await paymentService.GeneratePaymentNumberAsync(cancellationToken);
        return Ok(new { paymentNumber = number });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var payment = await paymentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        await paymentService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, CancellationToken cancellationToken)
    {
        await paymentService.CompleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/fail")]
    public async Task<IActionResult> Fail(int id, CancellationToken cancellationToken)
    {
        await paymentService.FailAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await paymentService.CancelAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/refund")]
    public async Task<IActionResult> Refund(int id, CancellationToken cancellationToken)
    {
        await paymentService.RefundAsync(id, cancellationToken);
        return NoContent();
    }
}
