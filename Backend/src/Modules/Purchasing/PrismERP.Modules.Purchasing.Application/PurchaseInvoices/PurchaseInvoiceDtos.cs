using PrismERP.Modules.Finance.Domain.Enums;

namespace PrismERP.Modules.Purchasing.Application.PurchaseInvoices;

public sealed record PurchaseInvoiceDto(
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
    IReadOnlyCollection<PurchaseInvoiceLineDto> Lines);

public sealed record PurchaseInvoiceLineDto(
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

public sealed record CreatePurchaseInvoiceRequest(
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
    List<CreatePurchaseInvoiceLineRequest> Lines
);

public sealed record UpdatePurchaseInvoiceRequest(
    DateTime? DueDate,
    string CounterpartyName,
    string CounterpartyAddress,
    int? SalesOrderId,
    int? DeliveryNoteId,
    int? PurchaseOrderId,
    int? GoodsReceiptId,
    string? Notes,
    List<UpdatePurchaseInvoiceLineRequest> Lines
    );

public sealed record CreatePurchaseInvoiceLineRequest(
    string? ProductCode,
    string? ProductName,
    string? Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal DiscountRate);

public sealed record UpdatePurchaseInvoiceLineRequest(
    string? ProductCode,
    string? ProductName,
    string? Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal DiscountRate);