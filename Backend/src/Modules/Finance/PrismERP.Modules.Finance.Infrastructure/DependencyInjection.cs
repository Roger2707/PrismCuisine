using Microsoft.Extensions.DependencyInjection;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Finance.Application.Abstractions.Persistence;
using PrismERP.Modules.Finance.Application.Invoices;
using PrismERP.Modules.Finance.Application.Payments;
using PrismERP.Modules.Finance.Infrastructure.Persistence;

namespace PrismERP.Modules.Finance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceModule(this IServiceCollection services)
    {
        services.AddSingleton<IModulePersistenceConfigurator, FinancePersistenceConfigurator>();
        services.AddScoped<IFinanceUnitOfWork, FinanceUnitOfWork>();
        services.AddScoped<IInvoiceRepository>(sp => sp.GetRequiredService<IFinanceUnitOfWork>().Invoices);
        services.AddScoped<IPaymentRepository>(sp => sp.GetRequiredService<IFinanceUnitOfWork>().Payments);
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IFinanceDataSeeder, FinanceDataSeeder>();

        return services;
    }
}
