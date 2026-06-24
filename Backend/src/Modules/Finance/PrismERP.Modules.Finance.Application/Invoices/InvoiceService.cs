using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Finance.Application.Abstractions.Persistence;
using PrismERP.Modules.Finance.Domain.Entities;

namespace PrismERP.Modules.Finance.Application.Invoices;

public sealed class InvoiceService(IFinanceUnitOfWork unitOfWork) : IInvoiceService
{
    #region Read
    public async Task<IReadOnlyCollection<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await unitOfWork.Invoices.GetAllAsync(cancellationToken);
        return invoices.Select(Map).ToList();
    }

    public async Task<InvoiceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await unitOfWork.Invoices.GetByIdAsync(id, cancellationToken);
        return invoice is null ? null : Map(invoice);
    }

    public async Task<InvoiceDto?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        var invoice = await unitOfWork.Invoices.GetByInvoiceNumberAsync(invoiceNumber, cancellationToken);
        return invoice is null ? null : Map(invoice);
    }

    public async Task<InvoiceDto?> GetByGoodsReceiptIdAsync(int goodsReceiptId, CancellationToken cancellationToken = default)
    {
        var invoice = await unitOfWork.Invoices.GetByGoodsReceiptIdAsync(goodsReceiptId, cancellationToken);
        return invoice is null ? null : Map(invoice);
    }

    public async Task<InvoiceDto?> GetByDeliveryNoteIdAsync(int deliveryNoteId, CancellationToken cancellationToken = default)
    {
        var invoice = await unitOfWork.Invoices.GetByDeliveryNoteIdAsync(deliveryNoteId, cancellationToken);
        return invoice is null ? null : Map(invoice);
    }

    public async Task<IReadOnlyCollection<InvoiceDto>> GetInvoicesByPurchaseOrderAsync(int purchaseOrderId, CancellationToken cancellationToken = default)
    {
        var invoices = await unitOfWork.Invoices.GetByPurchaseOrderAsync(purchaseOrderId, cancellationToken);
        return invoices.Select(Map).ToList();
    }

    #endregion

    #region Write

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await unitOfWork.Invoices.GetByInvoiceNumberAsync(request.InvoiceNumber, cancellationToken);
        if (existing is not null)
        {
            throw new ValidationException("invoiceNumber", $"Invoice number '{request.InvoiceNumber}' already exists.");
        }

        var invoice = Invoice.Create(
            request.InvoiceNumber,
            request.InvoiceType,
            request.InvoiceDate,
            request.DueDate,
            request.CounterpartyName,
            request.CounterpartyAddress,
            request.SalesOrderId,
            request.DeliveryNoteId,
            request.PurchaseOrderId,
            request.GoodsReceiptId,
            request.Notes);

        foreach (var line in request.Lines)
        {
            var invoiceLine = InvoiceLine.Create(
                line.ProductId,
                line.ProductName,
                line.Description,
                line.Quantity,
                line.UnitPrice,
                line.TaxRate,
                line.DiscountRate);

            invoice.AddLine(invoiceLine);
        }

        unitOfWork.Invoices.Add(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(invoice);
    }

    public async Task DeleteAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await unitOfWork.Invoices.GetByIdForUpdateAsync(invoiceId);
        if (invoice == null)
            throw new NotFoundException($"Invoice ID: {invoiceId} is not found !");

        unitOfWork.Invoices.Delete(invoice);
    }

    #endregion

    #region Business

    public async Task CancelAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await unitOfWork.Invoices.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Invoice '{id}' was not found.");

        invoice.Cancel();
        unitOfWork.Invoices.Update(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Helper Methods

    private static InvoiceDto Map(Invoice invoice) =>
        new(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.InvoiceType,
            invoice.Status,
            invoice.InvoiceDate,
            invoice.DueDate,
            invoice.CounterpartyName,
            invoice.CounterpartyAddress,
            invoice.SalesOrderId,
            invoice.DeliveryNoteId,
            invoice.PurchaseOrderId,
            invoice.GoodsReceiptId,
            invoice.SubTotal,
            invoice.TaxAmount,
            invoice.DiscountAmount,
            invoice.TotalAmount,
            invoice.PaidAmount,
            invoice.Notes,
            invoice.Lines.Select(Map).ToList());

    private static InvoiceLineDto Map(InvoiceLine line) =>
        new(
            line.Id,
            line.InvoiceId,
            line.ProductId,
            line.ProductName,
            line.Description,
            line.Quantity,
            line.UnitPrice,
            line.TaxRate,
            line.TaxAmount,
            line.DiscountRate,
            line.DiscountAmount,
            line.LineTotal);


    public async Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var count = await unitOfWork.Invoices.GetCountForDateAsync(today, cancellationToken);
        return $"INV-{today:yyyyMMdd}-{(count + 1):D4}";
    }

    #endregion
}
