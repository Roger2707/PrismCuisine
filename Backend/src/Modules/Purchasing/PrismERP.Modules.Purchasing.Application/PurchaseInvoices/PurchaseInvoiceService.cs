using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Finance.Application.Invoices;
using PrismERP.Modules.Finance.Domain.Enums;
using PrismERP.Modules.Inventory.Application.ProductCategories;
using PrismERP.Modules.Purchasing.Application.Abstractions;
using PrismERP.Modules.Purchasing.Application.GoodsReceipts;
using PrismERP.Modules.Purchasing.Application.Suppliers;
using PrismERP.Modules.Purchasing.Domain.Entities;
using PrismERP.Modules.Purchasing.Domain.Enums;

namespace PrismERP.Modules.Purchasing.Application.PurchaseInvoices;

public sealed class PurchaseInvoiceService(
    IPurchasingUnitOfWork unitOfWork,
    IGoodsReceiptService goodsReceiptService, 
    IInvoiceService invoiceService,
    IProductCategoryService productCategoryService,
    ISupplierService supplierService
) : IPurchaseInvoiceService
{
    public async Task<IReadOnlyCollection<PurchaseInvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invoices = (await invoiceService.GetAllAsync(cancellationToken)).Where(i => i.InvoiceType == InvoiceType.PurchaseInvoice).ToList();
        return invoices.Select(Map).ToList();      
    }

    public async Task<PurchaseInvoiceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceService.GetByIdAsync(id, cancellationToken);
        if((invoice != null && invoice.InvoiceType  != InvoiceType.PurchaseInvoice) || invoice == null)
            return null;

        return Map(invoice);
    }

    public async Task<PurchaseInvoiceDto?> GetByGoodsReceiptIdAsync(int goodsReceiptId, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceService.GetByGoodsReceiptIdAsync(goodsReceiptId, cancellationToken);
        if (invoice is null || invoice.InvoiceType != InvoiceType.PurchaseInvoice)
            return null;

        return Map(invoice);
    }

    public Task<PurchaseInvoiceDto> CreateFromGoodsReceiptAsync(
        CreatePurchaseInvoiceFromGoodsReceiptRequest request,
        CancellationToken cancellationToken = default) =>
        CreateInternalAsync(request.PurchaseOrderId, request.GoodsReceiptId, cancellationToken);

    public Task<PurchaseInvoiceDto> CreateAsync(CreatePurchaseInvoiceRequest request, CancellationToken cancellationToken = default) =>
        CreateInternalAsync(request.PurchaseOrderId, request.GoodsReceiptId, cancellationToken);

    private async Task<PurchaseInvoiceDto> CreateInternalAsync(
        int purchaseOrderId,
        int goodsReceiptId,
        CancellationToken cancellationToken)
    {
        #region Validations

        var existing = await invoiceService.GetByGoodsReceiptIdAsync(goodsReceiptId, cancellationToken);
        if (existing != null)
            throw new BusinessException($"Invoice with PurchaseOrderId : {purchaseOrderId} and GoodsReceiptId : {goodsReceiptId} is existed !");

        var goodsReceipt = await goodsReceiptService.GetByIdAsync(goodsReceiptId);
        if (goodsReceipt == null)
            throw new NotFoundException($"GoodsReceipt with ID : {goodsReceiptId} is not found !");
        var purchaseOrder = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(purchaseOrderId);
        if (purchaseOrder == null)
            throw new NotFoundException($"PurchaseOrder with ID : {goodsReceipt.PurchaseOrderId} is not found !");
        var supplier = await supplierService.GetByIdAsync(purchaseOrder.SupplierId);
        if (supplier == null)
            throw new NotFoundException($"Supplier with ID : {purchaseOrder.SupplierId} is not found !");

        #endregion

        var invocieLineRequests = new List<CreatePurchaseInvoiceLineRequest>(goodsReceipt.Lines.Count);
        foreach (var line in goodsReceipt.Lines)
        {
            // Note: because at the beginning project we haven't designed ProductName snapshot in DB
            var product = await productCategoryService.GetByIdAsync(line.ProductId);
            var invocieLineRequest = new CreatePurchaseInvoiceLineRequest(
                line.ProductId, product?.Name ?? "Temp ProductName", "", line.Quantity
                , line.UnitCost, 0, 0);

            invocieLineRequests.Add(invocieLineRequest);
        }

        PurchaseInvoiceDto purchaseInvoiceDto = null!;
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            // Create Invoice (AP) for Accounting
            var invoiceNumber = await invoiceService.GenerateInvoiceNumberAsync(cancellationToken);
            var invoiceLinesMap = invocieLineRequests.Select(Map).ToList();

            var invoiceDto = await invoiceService.CreateAsync(
                new CreateInvoiceRequest(
                    invoiceNumber, InvoiceType.PurchaseInvoice, DateTime.UtcNow, null, supplier.Name, supplier?.Address ?? "",
                    null, null, purchaseOrderId, goodsReceiptId, "", invoiceLinesMap), cancellationToken);

            // Update PurchaseOrder.InvoiceStatus
            purchaseOrder.UpdateInvoiceStatus();

            await unitOfWork.SaveChangesAsync(ct);
            purchaseInvoiceDto = Map(invoiceDto);

        }, cancellationToken);

        return purchaseInvoiceDto;
    }

    private PurchaseInvoiceDto Map(InvoiceDto invoice)
        => new(
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

    private static PurchaseInvoiceLineDto Map(InvoiceLineDto line) =>
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

    private static CreateInvoiceLineRequest Map(CreatePurchaseInvoiceLineRequest line)
        => new CreateInvoiceLineRequest(
                line.ProductId,
                line.ProductName,
                line.Description,
                line.Quantity,
                line.UnitPrice,
                line.TaxRate,
                line.DiscountRate);
}
