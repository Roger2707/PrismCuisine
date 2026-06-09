using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

namespace PrismERP.Modules.Inventory.Infrastructure.Persistence;

internal sealed class InventoryUnitOfWork(PrismERPDbContext db) : IInventoryUnitOfWork
{
    public IProductCategoryRepository ProductCategories { get; } = new ProductCategoryRepository(db);
    public IProductRepository Products { get; } = new ProductRepository(db);
    public IWarehouseRepository Warehouses { get; } = new WarehouseRepository(db);
    public IInventoryBalanceRepository Balances { get; } = new InventoryBalanceRepository(db);
    public IInventoryMovementRepository Movements { get; } = new InventoryMovementRepository(db);
    public IInventoryCostLayerRepository CostLayers { get; } = new InventoryCostLayerRepository(db);
    public IInventoryReservationRepository Reservations { get; } = new InventoryReservationRepository(db);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
