namespace PrismERP.Modules.SalesOrdering.Application.Deliveries;

public sealed record DeliveryNoteSummaryDto(
    int Id,
    string DeliveryNumber,
    int SalesOrderId,
    int CustomerId,
    string CustomerName,
    string OrderNumber,
    DateTime DeliveryDate,
    string Status,
    string? Notes
);

public sealed record DeliveryNoteDto(
    int Id,
    string DeliveryNumber,
    int SalesOrderId,
    int CustomerId,
    string CustomerName,
    string OrderNumber,
    DateTime DeliveryDate,
    string Status,
    string? Notes,
    IReadOnlyList<DeliveryNoteLineDto> Lines
);

public sealed record DeliveryNoteLineDto(
    int Id,
    int SalesOrderLineId,
    int ProductId,
    string ProductName,
    decimal QuantityDelivered
);

public sealed record CreateDeliveryNoteRequest(
    int SalesOrderId,
    string? Notes,
    IReadOnlyList<CreateDeliveryNoteLineRequest> Lines);

public sealed record CreateDeliveryNoteLineRequest(
    int SalesOrderLineId,
    decimal QuantityDelivered);

public sealed record UpdateDeliveryNoteRequest(
    string? Notes,
    IReadOnlyList<CreateDeliveryNoteLineRequest> Lines);
