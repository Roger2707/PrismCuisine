using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Finance.Infrastructure;
using PrismERP.Modules.Inventory.Domain.Entities;
using PrismERP.Modules.Inventory.Domain.Enums;
using PrismERP.Modules.Inventory.Infrastructure;
using PrismERP.Modules.Purchasing.Application.GoodsReceipts;
using PrismERP.Modules.Purchasing.Application.PurchaseOrders;
using PrismERP.Modules.Purchasing.Domain.Entities;
using PrismERP.Modules.Purchasing.Infrastructure;

namespace PrismERP.Purchasing.IntegrationTests;

public sealed class TestDatabaseFixture : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;

    public TestDatabaseFixture()
    {
        ConnectionString = Environment.GetEnvironmentVariable("PRISMERP_TEST_CONNECTION_STRING")
            ?? "Server=localhost;Database=PrismERP_Test;User Id=sa;Password=admin1;TrustServerCertificate=True;MultipleActiveResultSets=True";

        var services = new ServiceCollection();
        services.AddPurchasingModule();
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

    public Task<TestPurchasingSeed> SeedMasterDataAsync()
        => ExecuteInScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<PrismERPDbContext>();

            var category = ProductCategory.Create("TEST-PO", "Test Category");
            var warehouse = Warehouse.Create("WH-PO", "PO Test Warehouse");
            db.ProductCategories.Add(category);
            db.Warehouses.Add(warehouse);

            var supplier = Supplier.Create("SUP-TEST", "Test Supplier");
            db.Suppliers.Add(supplier);

            await db.SaveChangesAsync();

            var product = Product.Create(category.Id, "SKU-PO-TEST", "PO Test Product", "PCS");
            db.Products.Add(product);

            await db.SaveChangesAsync();

            return new TestPurchasingSeed(
                product.Id,
                warehouse.Id,
                supplier.Id);
        });

    public Task<PurchaseOrderDto> CreatePurchaseOrderAsync(
        TestPurchasingSeed seed,
        decimal quantity,
        decimal unitPrice = 50m)
        => ExecuteInScopeAsync(async sp =>
        {
            var service = sp.GetRequiredService<IPurchaseOrderService>();

            return await service.CreateAsync(new CreatePurchaseOrderRequest(
                seed.SupplierId,
                seed.WarehouseId,
                "Integration test PO",
                [
                    new CreatePurchaseOrderLineRequest(
                        seed.ProductId,
                        quantity,
                        unitPrice)
                ]));
        });

    public Task<PurchaseOrderDto?> GetPurchaseOrderAsync(int purchaseOrderId)
        => ExecuteInScopeAsync(sp =>
            sp.GetRequiredService<IPurchaseOrderService>().GetByIdAsync(purchaseOrderId));

    public Task<GoodsReceiptDto> CreateGoodsReceiptAsync(
        int purchaseOrderId,
        int purchaseOrderLineId,
        decimal quantity,
        decimal? unitCost = null)
        => ExecuteInScopeAsync(async sp =>
        {
            var service = sp.GetRequiredService<IGoodsReceiptService>();

            return await service.CreateAsync(new CreateGoodsReceiptRequest(
                purchaseOrderId,
                "Integration test GR",
                [new AddGoodsReceiptLineRequest(purchaseOrderLineId, quantity, unitCost)]));
        });

    public Task<GoodsReceiptDto?> GetGoodsReceiptAsync(int goodsReceiptId)
        => ExecuteInScopeAsync(sp =>
            sp.GetRequiredService<IGoodsReceiptService>().GetByIdAsync(goodsReceiptId));

    public Task<InventoryBalance?> GetBalanceByProductWarehouseAsync(int productId, int warehouseId)
        => ExecuteInScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<PrismERPDbContext>();
            return await db.InventoryBalances
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.ProductId == productId && b.WarehouseId == warehouseId);
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
}

public sealed record TestPurchasingSeed(
    int ProductId,
    int WarehouseId,
    int SupplierId);
