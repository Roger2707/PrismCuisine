using PrismERP.Modules.Finance.Domain.Enums;

namespace PrismERP.Modules.Finance.Application.Invoices;

public sealed record InvoiceDto(
    int Id,
    string InvoiceNumber,
    InvoiceType InvoiceType,
    InvoiceStatus Status,
    DateTime InvoiceDate,
    DateTime? DueDate,
    string? CustomerName,
    string? CustomerAddress,
    string? CustomerTaxId,
    decimal SubTotal,
    decimal TaxAmount,
    decimal DiscountAmount,
    decimal TotalAmount,
    decimal PaidAmount,
    string? Notes,
    IReadOnlyCollection<InvoiceLineDto> Lines);

public sealed record InvoiceLineDto(
    int Id,
    int InvoiceId,
    string? ProductCode,
    string? ProductName,
    string? Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal TaxAmount,
    decimal DiscountRate,
    decimal DiscountAmount,
    decimal LineTotal);

public sealed record CreateInvoiceRequest(
    string InvoiceNumber,
    InvoiceType InvoiceType,
    DateTime InvoiceDate,
    DateTime? DueDate,
    string? CustomerName,
    string? CustomerAddress,
    string? CustomerTaxId,
    string? Notes);

public sealed record UpdateInvoiceRequest(
    DateTime? DueDate,
    string? CustomerName,
    string? CustomerAddress,
    string? CustomerTaxId,
    string? Notes);

public sealed record CreateInvoiceLineRequest(
    int InvoiceId,
    string? ProductCode,
    string? ProductName,
    string? Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal DiscountRate);

public sealed record UpdateInvoiceLineRequest(
    string? ProductCode,
    string? ProductName,
    string? Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal DiscountRate);
