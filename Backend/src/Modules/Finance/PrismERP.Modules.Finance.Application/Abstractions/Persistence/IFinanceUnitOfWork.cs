using PrismERP.BuildingBlocks.Application.Abstractions.Persistence;

namespace PrismERP.Modules.Finance.Application.Abstractions.Persistence;

public interface IFinanceUnitOfWork : IUnitOfWork
{
    IInvoiceRepository Invoices { get; }
    IInvoiceLineRepository InvoiceLines { get; }
    IPaymentRepository Payments { get; }
}
