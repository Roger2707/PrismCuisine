using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;

namespace PrismCuisine.Modules.Inventory.Infrastructure.Persistence;

internal sealed class InventoryPersistenceConfigurator : IModulePersistenceConfigurator
{
    public void Configure(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryPersistenceConfigurator).Assembly);
}
