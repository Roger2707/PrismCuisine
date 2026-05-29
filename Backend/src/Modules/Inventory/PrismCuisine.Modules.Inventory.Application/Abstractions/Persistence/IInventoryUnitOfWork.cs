using PrismCuisine.BuildingBlocks.Application.Abstractions.Persistence;

namespace PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;

public interface IInventoryUnitOfWork : IUnitOfWork
{
    IProductCategoryRepository ProductCategories { get; }
    IProductRepository Products { get; }
    IWarehouseRepository Warehouses { get; }
    IInventoryBalanceRepository Balances { get; }
    IInventoryMovementRepository Movements { get; }
    IInventoryCostLayerRepository CostLayers { get; }
    IInventoryReservationRepository Reservations { get; }
}
