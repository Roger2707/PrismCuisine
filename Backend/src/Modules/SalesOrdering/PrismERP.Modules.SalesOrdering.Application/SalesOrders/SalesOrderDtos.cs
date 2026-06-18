namespace PrismERP.Modules.SalesOrdering.Application.SalesOrders;

public sealed record SalesOrderSummaryDto(
    int Id,
    string OrderNumber,
    int CustomerId,
    string CustomerName,
    DateTime OrderDate,
    DateTime? DeliveryDate,
    DateTime? ApprovedAt,
    string Status,
    string InvoiceStatus,
    string? Notes,
    decimal SubTotal,
    decimal TotalDiscount,
    decimal TotalVAT,
    decimal TotalAmount
);

public sealed record SalesOrderDto(
    int Id,
    string OrderNumber,
    int CustomerId,
    string CustomerName,
    DateTime OrderDate,
    DateTime? DeliveryDate,
    DateTime? ApprovedAt,
    string Status,
    string InvoiceStatus,
    string? Notes,
    decimal SubTotal,
    decimal TotalDiscount,
    decimal TotalVAT,
    decimal TotalAmount,
    IReadOnlyList<SalesOrderLineDto> Lines
);

public sealed record SalesOrderLineDto(
    int Id,
    int ProductId,
    string ProductName,
    decimal QuantityOrdered,
    decimal QuantityDelivered,
    decimal QuantityRemaining,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal VATRate,
    decimal DiscountAmount,
    decimal VATAmount,
    decimal LineTotal
);

public sealed record CreateSalesOrderRequest(
    int CustomerId,
    string? CustomerName,
    string? Notes,
    IReadOnlyList<CreateSalesOrderLineRequest> Lines
);

public sealed record CreateSalesOrderLineRequest(
    int ProductId,
    string ProductName,
    decimal QuantityOrdered,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal VATRate
);

public sealed record UpdateSalesOrderRequest(
    int CustomerId,
    string? CustomerName,
    string? Notes,
    IReadOnlyList<CreateSalesOrderLineRequest> Lines);
