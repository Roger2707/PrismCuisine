namespace PrismERP.Modules.Finance.Application.Payments;

public interface IPaymentService
{
    Task<IReadOnlyCollection<PaymentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PaymentDto>> GetByInvoiceIdAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task<PaymentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PaymentDto?> GetByPaymentNumberAsync(string paymentNumber, CancellationToken cancellationToken = default);
    Task<PaymentDto> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdatePaymentRequest request, CancellationToken cancellationToken = default);
    Task CompleteAsync(int id, CancellationToken cancellationToken = default);
    Task FailAsync(int id, CancellationToken cancellationToken = default);
    Task CancelAsync(int id, CancellationToken cancellationToken = default);
    Task RefundAsync(int id, CancellationToken cancellationToken = default);
    Task<string> GeneratePaymentNumberAsync(CancellationToken cancellationToken = default);
}
