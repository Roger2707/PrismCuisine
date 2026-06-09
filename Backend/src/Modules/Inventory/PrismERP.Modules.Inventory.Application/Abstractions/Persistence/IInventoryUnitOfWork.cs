using PrismERP.BuildingBlocks.Application.Abstractions.Persistence;

namespace PrismERP.Modules.Inventory.Application.Abstractions.Persistence;

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
