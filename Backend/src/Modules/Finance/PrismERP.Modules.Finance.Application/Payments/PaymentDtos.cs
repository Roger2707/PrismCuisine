using PrismERP.Modules.Finance.Domain.Enums;

namespace PrismERP.Modules.Finance.Application.Payments;

public sealed record PaymentDto(
    int Id,
    int InvoiceId,
    string PaymentNumber,
    PaymentMethod PaymentMethod,
    PaymentStatus Status,
    decimal Amount,
    DateTime PaymentDate,
    string? ReferenceNumber,
    string? BankName,
    string? AccountNumber,
    string? Notes);

public sealed record CreatePaymentRequest(
    int InvoiceId,
    string PaymentNumber,
    PaymentMethod PaymentMethod,
    decimal Amount,
    DateTime PaymentDate,
    string? ReferenceNumber,
    string? BankName,
    string? AccountNumber,
    string? Notes);

public sealed record UpdatePaymentRequest(
    PaymentMethod PaymentMethod,
    string? ReferenceNumber,
    string? BankName,
    string? AccountNumber,
    string? Notes);
