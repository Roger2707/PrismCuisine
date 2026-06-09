using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;

namespace PrismERP.Modules.Inventory.Infrastructure.Persistence;

internal sealed class InventoryPersistenceConfigurator : IModulePersistenceConfigurator
{
    public void Configure(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryPersistenceConfigurator).Assembly);
}
