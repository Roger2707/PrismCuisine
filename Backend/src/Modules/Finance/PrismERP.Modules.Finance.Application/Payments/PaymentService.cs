using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Finance.Application.Abstractions.Persistence;
using PrismERP.Modules.Finance.Domain.Entities;
using PrismERP.Modules.Finance.Domain.Enums;

namespace PrismERP.Modules.Finance.Application.Payments;

public sealed class PaymentService(IFinanceUnitOfWork unitOfWork) : IPaymentService
{
    public async Task<IReadOnlyCollection<PaymentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var payments = await unitOfWork.Payments.GetAllAsync(cancellationToken);
        return payments.Select(Map).ToList();
    }

    public async Task<IReadOnlyCollection<PaymentDto>> GetByInvoiceIdAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        var payments = await unitOfWork.Payments.GetByInvoiceIdAsync(invoiceId, cancellationToken);
        return payments.Select(Map).ToList();
    }

    public async Task<PaymentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var payment = await unitOfWork.Payments.GetByIdAsync(id, cancellationToken);
        return payment is null ? null : Map(payment);
    }

    public async Task<PaymentDto?> GetByPaymentNumberAsync(string paymentNumber, CancellationToken cancellationToken = default)
    {
        var payment = await unitOfWork.Payments.GetByPaymentNumberAsync(paymentNumber, cancellationToken);
        return payment is null ? null : Map(payment);
    }

    public async Task<PaymentDto> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var invoice = await unitOfWork.Invoices.GetByIdAsync(request.InvoiceId, cancellationToken)
            ?? throw new NotFoundException($"Invoice '{request.InvoiceId}' was not found.");

        var existingByPaymentNumber = await unitOfWork.Payments.GetByPaymentNumberAsync(request.PaymentNumber, cancellationToken);
        if (existingByPaymentNumber is not null)
            throw new BusinessException($"Payment number '{request.PaymentNumber}' already exists.");

        var existingByInvoice = await unitOfWork.Payments.GetByInvoiceIdAsync(request.InvoiceId, cancellationToken);
        if(existingByInvoice is not null)
        {
            if (existingByInvoice.Any(p => p.Status == PaymentStatus.Completed))
                throw new BusinessException($"Payment with InvoiceId : {request.InvoiceId} has Paid !");
        }

        var payment = Payment.Create(
            request.InvoiceId,
            request.PaymentNumber,
            request.PaymentMethod,
            request.Amount,
            request.PaymentDate,
            request.ReferenceNumber,
            request.BankName,
            request.AccountNumber,
            request.Notes);

        unitOfWork.Payments.Add(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(payment);
    }

    public async Task UpdateAsync(int id, UpdatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var payment = await unitOfWork.Payments.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Payment '{id}' was not found.");

        payment.Update(
            request.PaymentMethod,
            request.ReferenceNumber,
            request.BankName,
            request.AccountNumber,
            request.Notes);

        unitOfWork.Payments.Update(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var payment = await unitOfWork.Payments.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Payment '{id}' was not found.");

        payment.Complete();

        var invoice = await unitOfWork.Invoices.GetByIdForUpdateAsync(payment.InvoiceId, cancellationToken)
            ?? throw new NotFoundException($"Invoice '{payment.InvoiceId}' was not found.");

        invoice.AddPayment(payment.Amount);
        unitOfWork.Payments.Update(payment);
        unitOfWork.Invoices.Update(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task FailAsync(int id, CancellationToken cancellationToken = default)
    {
        var payment = await unitOfWork.Payments.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Payment '{id}' was not found.");

        payment.Fail();
        unitOfWork.Payments.Update(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(int id, CancellationToken cancellationToken = default)
    {
        var payment = await unitOfWork.Payments.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Payment '{id}' was not found.");

        payment.Cancel();
        unitOfWork.Payments.Update(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RefundAsync(int id, CancellationToken cancellationToken = default)
    {
        var payment = await unitOfWork.Payments.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Payment '{id}' was not found.");

        payment.Refund();
        unitOfWork.Payments.Update(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static PaymentDto Map(Payment payment) =>
        new(
            payment.Id,
            payment.InvoiceId,
            payment.PaymentNumber,
            payment.PaymentMethod,
            payment.Status,
            payment.Amount,
            payment.PaymentDate,
            payment.ReferenceNumber,
            payment.BankName,
            payment.AccountNumber,
            payment.Notes);

    public async Task<string> GeneratePaymentNumberAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var count = await unitOfWork.Payments.GetCountForDateAsync(today, cancellationToken);
        return $"PAY-{today:yyyyMMdd}-{(count + 1):D4}";
    }
}
