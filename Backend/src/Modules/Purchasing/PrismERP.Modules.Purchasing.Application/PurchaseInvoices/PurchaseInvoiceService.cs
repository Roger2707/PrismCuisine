using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Finance.Application.Invoices;
using PrismERP.Modules.Finance.Domain.Enums;
using PrismERP.Modules.Inventory.Application.ProductCategories;
using PrismERP.Modules.Purchasing.Application.Abstractions;
using PrismERP.Modules.Purchasing.Application.GoodsReceipts;
using PrismERP.Modules.Purchasing.Application.PurchaseOrders;
using PrismERP.Modules.Purchasing.Application.Suppliers;
using PrismERP.Modules.Purchasing.Domain.Entities;
using PrismERP.Modules.Purchasing.Domain.Enums;

namespace PrismERP.Modules.Purchasing.Application.PurchaseInvoices;

public sealed class PurchaseInvoiceService(
    IPurchasingUnitOfWork unitOfWork,
    IPurchaseOrderService purchaseOrderService, 
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

    public async Task<PurchaseInvoiceDto> CreateAsync(CreatePurchaseInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        #region Validations

        var existing = await invoiceService.GetByGoodsReceiptIdAsync(request.GoodsReceiptId, cancellationToken);
        if (existing != null)
            throw new BusinessException($"Invoice with PurchaseOrderId : {request.PurchaseOrderId} and GoodsReceiptId : {request.GoodsReceiptId} is existed !");

        var goodsReceipt = await goodsReceiptService.GetByIdAsync(request.GoodsReceiptId);
        if (goodsReceipt == null)
            throw new NotFoundException($"GoodsReceipt with ID : {request.GoodsReceiptId} is not found !");
        var purchaseOrder = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(request.PurchaseOrderId);
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
                    invoiceNumber, InvoiceType.SalesInvoice, DateTime.UtcNow, null, supplier.Name, supplier?.Address ?? "",
                    null, null, request.PurchaseOrderId, request.GoodsReceiptId, "", invoiceLinesMap), cancellationToken);

            // Update InvoiceStatus (in PurchaseOrder)
            await UpdatePurchaseInvoiceStatus(purchaseOrder);

            await unitOfWork.SaveChangesAsync(ct);
            purchaseInvoiceDto = Map(invoiceDto);

        }, cancellationToken);

        return purchaseInvoiceDto;
    }

    private async Task UpdatePurchaseInvoiceStatus(PurchaseOrder purchaseOrder)
    {
        var invoices = await invoiceService.GetInvoicesByPurchaseOrderAsync(purchaseOrder.Id);
        if (invoices == null || !invoices.Any() || purchaseOrder.Lines == null || !purchaseOrder.Lines.Any())
            return;

        var totalInvoicedQtyByProduct = invoices
                .SelectMany(inv => inv.Lines)
                .GroupBy(invLine => invLine.ProductId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Sum(invLine => invLine.Quantity)
                );

        bool isAllLinesFullyInvoiced = true;
        foreach (var pLine in purchaseOrder.Lines)
        {
            totalInvoicedQtyByProduct.TryGetValue(pLine.ProductId, out decimal totalInvoicedQty);
            if (totalInvoicedQty != pLine.QuantityOrdered)
            {
                isAllLinesFullyInvoiced = false;
                break;
            }
        }

        purchaseOrder.UpdateInvoiceStatus(
            isAllLinesFullyInvoiced ? 
            PurchaseOrderInvoicingStatus.FullyInvoiced : PurchaseOrderInvoicingStatus.PartiallyInvoiced
        );
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
