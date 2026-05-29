using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;
using PrismCuisine.Modules.Inventory.Infrastructure.Persistence.Repositories;

namespace PrismCuisine.Modules.Inventory.Infrastructure.Persistence;

internal sealed class InventoryUnitOfWork(PrismCuisineDbContext db) : IInventoryUnitOfWork
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
