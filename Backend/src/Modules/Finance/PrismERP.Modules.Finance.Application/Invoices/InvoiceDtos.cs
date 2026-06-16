using PrismERP.Modules.Finance.Domain.Enums;

namespace PrismERP.Modules.Finance.Application.Invoices;

public sealed record InvoiceDto(
    int Id,
    string InvoiceNumber,
    InvoiceType InvoiceType,
    InvoiceStatus Status,
    DateTime InvoiceDate,
    DateTime? DueDate,
    string? CounterpartyName,
    string? CounterpartyAddress,
    int? SalesOrderId,
    int? DeliveryNoteId,
    int? PurchaseOrderId,
    int? GoodsReceiptId,
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
    int ProductId,
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
    string CounterpartyName,
    string CounterpartyAddress,
    int? SalesOrderId,
    int? DeliveryNoteId,
    int? PurchaseOrderId,
    int? GoodsReceiptId,
    string? Notes,
    List<CreateInvoiceLineRequest> Lines
);

public sealed record CreateInvoiceLineRequest(
    int ProductId,
    string? ProductName,
    string? Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal DiscountRate);
