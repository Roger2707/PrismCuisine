using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;

namespace PrismERP.Modules.SalesOrdering.Infrastructure.Persistence;

internal sealed class SalesOrderPersistenceConfigurator : IModulePersistenceConfigurator
{
    public void Configure(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesOrderPersistenceConfigurator).Assembly);
}
