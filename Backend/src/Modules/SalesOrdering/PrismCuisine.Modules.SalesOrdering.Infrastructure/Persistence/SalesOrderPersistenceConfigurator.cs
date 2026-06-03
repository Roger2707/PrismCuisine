using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;

namespace PrismCuisine.Modules.SalesOrdering.Infrastructure.Persistence;

internal sealed class SalesOrderPersistenceConfigurator : IModulePersistenceConfigurator
{
    public void Configure(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesOrderPersistenceConfigurator).Assembly);
}
