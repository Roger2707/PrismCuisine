using Microsoft.Extensions.DependencyInjection;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.SalesOrdering.Application.Abtractions;
using PrismERP.Modules.SalesOrdering.Application.Customers;
using PrismERP.Modules.SalesOrdering.Application.Deliveries;
using PrismERP.Modules.SalesOrdering.Application.SalesOrders;
using PrismERP.Modules.SalesOrdering.Infrastructure.Persistence;

namespace PrismERP.Modules.SalesOrdering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSalesOrderModule(this IServiceCollection services)
    {
        services.AddSingleton<IModulePersistenceConfigurator, SalesOrderPersistenceConfigurator>();
        services.AddScoped<ISalesOrderingUnitOfWork, SalesOrderingUnitOfWork>();
        services.AddScoped<ICustomerRepository>(sp => sp.GetRequiredService<ISalesOrderingUnitOfWork>().Customers);
        services.AddScoped<ISalesOrderRepository>(sp => sp.GetRequiredService<ISalesOrderingUnitOfWork>().SalesOrders);
        services.AddScoped<IDeliveryNoteRepository>(sp => sp.GetRequiredService<ISalesOrderingUnitOfWork>().DeliveryNotes);

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISalesOrderService, SalesOrderService>();
        services.AddScoped<IDeliveryNoteService, DeliveryNoteService>();
        services.AddScoped<ISalesOrderingDataSeeder, SalesOrderingDataSeeder>();

        return services;
    }
}
