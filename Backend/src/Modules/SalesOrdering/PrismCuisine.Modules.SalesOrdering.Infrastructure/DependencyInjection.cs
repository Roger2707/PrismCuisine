using Microsoft.Extensions.DependencyInjection;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.SalesOrdering.Application.Abtractions;
using PrismCuisine.Modules.SalesOrdering.Application.Customers;
using PrismCuisine.Modules.SalesOrdering.Application.SalesOrders;
using PrismCuisine.Modules.SalesOrdering.Infrastructure.Persistence;

namespace PrismCuisine.Modules.SalesOrdering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSalesOrderModule(this IServiceCollection services)
    {
        services.AddSingleton<IModulePersistenceConfigurator, SalesOrderPersistenceConfigurator>();
        services.AddScoped<ISalesOrderingUnitOfWork, SalesOrderingUnitOfWork>();
        services.AddScoped<ICustomerRepository>(sp => sp.GetRequiredService<ISalesOrderingUnitOfWork>().Customers);
        services.AddScoped<ISalesOrderRepository>(sp => sp.GetRequiredService<ISalesOrderingUnitOfWork>().SalesOrders);

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISalesOrderService, SalesOrderService>();
        services.AddScoped<ISalesOrderingDataSeeder, SalesOrderingDataSeeder>();

        return services;
    }
}
