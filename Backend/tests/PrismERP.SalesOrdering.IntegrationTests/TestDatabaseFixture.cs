using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Finance.Application.Invoices;
using PrismERP.Modules.Finance.Domain.Entities;
using PrismERP.Modules.Inventory.Domain.Entities;
using PrismERP.Modules.Inventory.Domain.Enums;
using PrismERP.Modules.Inventory.Infrastructure;
using PrismERP.Modules.SalesOrdering.Application.Deliveries;
using PrismERP.Modules.SalesOrdering.Application.SalesOrders;
using PrismERP.Modules.SalesOrdering.Domain.Entities;
using PrismERP.Modules.SalesOrdering.Infrastructure;
using PrismERP.Modules.Finance.Infrastructure;

namespace PrismERP.SalesOrdering.IntegrationTests;

public sealed class TestDatabaseFixture : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;

    public TestDatabaseFixture()
    {
        ConnectionString = Environment.GetEnvironmentVariable("PRISMERP_TEST_CONNECTION_STRING")
            ?? "Server=localhost;Database=PrismERP_Test;User Id=sa;Password=admin1;TrustServerCertificate=True;MultipleActiveResultSets=True";

        var services = new ServiceCollection();
        services.AddSalesOrderModule();
        services.AddInventoryModule();
        services.AddFinanceModule();
        services.AddDbContext<PrismERPDbContext>(options =>
            options.UseSqlServer(ConnectionString, sql =>
            {
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
                sql.CommandTimeout(60);
            }));

        _serviceProvider = services.BuildServiceProvider(validateScopes: true);
    }

    public string ConnectionString { get; }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await ExecuteInScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<PrismERPDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        });
    }

    public async Task ExecuteInScopeAsync(Func<IServiceProvider, Task> action)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await action(scope.ServiceProvider);
    }

    public async Task<T> ExecuteInScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        return await action(scope.ServiceProvider);
    }

    public Task<SalesOrderDto> CreateSalesOrderAsync(
        int productId,
        string productName,
        decimal quantity,
        decimal unitPrice = 100m)
        => ExecuteInScopeAsync(async sp =>
        {
            var customerId = await EnsureCustomerAsync(sp);
            var service = sp.GetRequiredService<ISalesOrderService>();

            return await service.CreateAsync(new CreateSalesOrderRequest(
                customerId,
                "Use persisted customer name",
                "Integration test SO",
                [
                    new CreateSalesOrderLineRequest(
                        productId,
                        productName,
                        quantity,
                        unitPrice,
                        0m,
                        10m)
                ]));
        });

    public Task<SalesOrderDto?> GetSalesOrderAsync(int salesOrderId)
        => ExecuteInScopeAsync(sp => sp.GetRequiredService<ISalesOrderService>().GetByIdAsync(salesOrderId));

    public Task<DeliveryNoteDto> CreateDeliveryNoteAsync(
        int salesOrderId,
        int salesOrderLineId,
        decimal quantityDelivered)
        => ExecuteInScopeAsync(async sp =>
        {
            var service = sp.GetRequiredService<IDeliveryNoteService>();

            return await service.CreateAsync(new CreateDeliveryNoteRequest(
                salesOrderId,
                "Integration test DN",
                [new CreateDeliveryNoteLineRequest(salesOrderLineId, quantityDelivered)]));
        });

    public async Task<TestInventorySeed> SeedInventoryAsync(
        params (decimal Quantity, decimal UnitCost)[] layers)
    {
        if (layers.Length == 0)
            throw new ArgumentException("At least one cost layer is required.", nameof(layers));

        return await ExecuteInScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<PrismERPDbContext>();

            var category = ProductCategory.Create("TEST", "Test Category");
            var warehouse = Warehouse.Create("MAIN", "Main Warehouse");
            db.ProductCategories.Add(category);
            db.Warehouses.Add(warehouse);
            await db.SaveChangesAsync();

            var product = Product.Create(category.Id, "SKU-TEST", "Test Product", "PCS");
            db.Products.Add(product);
            await db.SaveChangesAsync();

            var balance = InventoryBalance.Create(product.Id, warehouse.Id, reorderLevel: 0m);
            balance.Increase(layers.Sum(l => l.Quantity));
            db.InventoryBalances.Add(balance);
            await db.SaveChangesAsync();

            var receivedAt = DateTime.UtcNow.AddDays(-layers.Length);
            foreach (var layer in layers.Select((value, index) => new { value, index }))
            {
                db.InventoryCostLayers.Add(InventoryCostLayer.Create(
                    balance.Id,
                    layer.value.Quantity,
                    layer.value.UnitCost,
                    receivedAt.AddDays(layer.index)));
            }

            await db.SaveChangesAsync();
            return new TestInventorySeed(product.Id, product.Name, warehouse.Id, balance.Id);
        });
    }

    public Task<InventoryBalance> GetBalanceAsync(int balanceId)
        => ExecuteInScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<PrismERPDbContext>();
            return await db.InventoryBalances.AsNoTracking().SingleAsync(b => b.Id == balanceId);
        });

    public Task<List<InventoryCostLayer>> GetCostLayersAsync(int balanceId)
        => ExecuteInScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<PrismERPDbContext>();
            return await db.InventoryCostLayers
                .AsNoTracking()
                .Where(l => l.InventoryBalanceId == balanceId)
                .OrderBy(l => l.ReceivedAt)
                .ToListAsync();
        });

    public Task<List<InventoryMovement>> GetMovementsAsync(int balanceId, InventoryMovementType movementType)
        => ExecuteInScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<PrismERPDbContext>();
            return await db.InventoryMovements
                .AsNoTracking()
                .Where(m => m.InventoryBalanceId == balanceId && m.MovementType == movementType)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        });

    public Task<List<InventoryReservation>> GetReservationsAsync(int balanceId)
        => ExecuteInScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<PrismERPDbContext>();
            return await db.InventoryReservations
                .AsNoTracking()
                .Where(r => r.InventoryBalanceId == balanceId)
                .OrderBy(r => r.Id)
                .ToListAsync();
        });

    public Task<List<Invoice>> GetInvoicesByDeliveryNoteAsync(int deliveryNoteId)
        => ExecuteInScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<PrismERPDbContext>();
            return await db.Invoices
                .AsNoTracking()
                .Where(i => i.DeliveryNoteId == deliveryNoteId)
                .OrderBy(i => i.Id)
                .ToListAsync();
        });

    private static async Task<int> EnsureCustomerAsync(IServiceProvider sp)
    {
        var db = sp.GetRequiredService<PrismERPDbContext>();
        var existing = await db.Customers.FirstOrDefaultAsync(c => c.Code == "CUST-TEST");
        if (existing is not null)
            return existing.Id;

        var customer = Customer.Create("CUST-TEST", "Test Customer");
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return customer.Id;
    }
}

public sealed record TestInventorySeed(
    int ProductId,
    string ProductName,
    int WarehouseId,
    int BalanceId);
