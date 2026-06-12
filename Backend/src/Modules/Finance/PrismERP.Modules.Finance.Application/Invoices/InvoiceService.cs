using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Finance.Application.Abstractions.Persistence;
using PrismERP.Modules.Finance.Domain.Entities;

namespace PrismERP.Modules.Finance.Application.Invoices;

public sealed class InvoiceService(IFinanceUnitOfWork unitOfWork) : IInvoiceService
{
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
            request.CustomerName,
            request.CustomerAddress,
            request.Notes);

        foreach (var line in request.Lines)
        {
            var invoiceLine = InvoiceLine.Create(
                line.ProductCode,
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

    public async Task UpdateAsync(int id, UpdateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var invoice = await unitOfWork.Invoices.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Invoice '{id}' was not found.");

        invoice.Update(
            request.DueDate,
            request.CustomerName,
            request.CustomerAddress,
            request.Notes);

        unitOfWork.Invoices.Update(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task PostAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await unitOfWork.Invoices.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Invoice '{id}' was not found.");

        invoice.Post();
        unitOfWork.Invoices.Update(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await unitOfWork.Invoices.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Invoice '{id}' was not found.");

        invoice.Cancel();
        unitOfWork.Invoices.Update(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AddLineAsync(int invoiceId, CreateInvoiceLineRequest request, CancellationToken cancellationToken = default)
    {
        var invoice = await unitOfWork.Invoices.GetByIdForUpdateAsync(invoiceId, cancellationToken)
            ?? throw new NotFoundException($"Invoice '{invoiceId}' was not found.");

        var line = InvoiceLine.Create(
            request.ProductCode,
            request.ProductName,
            request.Description,
            request.Quantity,
            request.UnitPrice,
            request.TaxRate,
            request.DiscountRate);

        invoice.AddLine(line);
        unitOfWork.Invoices.Update(invoice);
        unitOfWork.InvoiceLines.Add(line);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateLineAsync(int invoiceId, int lineId, UpdateInvoiceLineRequest request, CancellationToken cancellationToken = default)
    {
        var invoice = await unitOfWork.Invoices.GetByIdForUpdateAsync(invoiceId, cancellationToken)
            ?? throw new NotFoundException($"Invoice '{invoiceId}' was not found.");

        var line = await unitOfWork.InvoiceLines.GetByIdAsync(lineId, cancellationToken)
            ?? throw new NotFoundException($"Invoice line '{lineId}' was not found.");

        if (line.InvoiceId != invoiceId)
        {
            throw new ValidationException("lineId", $"Invoice line '{lineId}' does not belong to invoice '{invoiceId}'.");
        }

        line.Update(
            request.ProductCode,
            request.ProductName,
            request.Description,
            request.Quantity,
            request.UnitPrice,
            request.TaxRate,
            request.DiscountRate);

        invoice.UpdateLine(line);
        unitOfWork.Invoices.Update(invoice);
        unitOfWork.InvoiceLines.Update(line);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveLineAsync(int invoiceId, int lineId, CancellationToken cancellationToken = default)
    {
        var invoice = await unitOfWork.Invoices.GetByIdForUpdateAsync(invoiceId, cancellationToken)
            ?? throw new NotFoundException($"Invoice '{invoiceId}' was not found.");

        var line = await unitOfWork.InvoiceLines.GetByIdAsync(lineId, cancellationToken)
            ?? throw new NotFoundException($"Invoice line '{lineId}' was not found.");

        if (line.InvoiceId != invoiceId)
        {
            throw new ValidationException("lineId", $"Invoice line '{lineId}' does not belong to invoice '{invoiceId}'.");
        }

        invoice.RemoveLine(line);
        unitOfWork.Invoices.Update(invoice);
        unitOfWork.InvoiceLines.Delete(line);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static InvoiceDto Map(Invoice invoice) =>
        new(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.InvoiceType,
            invoice.Status,
            invoice.InvoiceDate,
            invoice.DueDate,
            invoice.CustomerName,
            invoice.CustomerAddress,
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
            line.ProductCode,
            line.ProductName,
            line.Description,
            line.Quantity,
            line.UnitPrice,
            line.TaxRate,
            line.TaxAmount,
            line.DiscountRate,
            line.DiscountAmount,
            line.LineTotal);

    #region Helper Methods

    public async Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var count = await unitOfWork.Invoices.GetCountForDateAsync(today, cancellationToken);
        return $"INV-{today:yyyyMMdd}-{(count + 1):D4}";
    }

    #endregion
}
