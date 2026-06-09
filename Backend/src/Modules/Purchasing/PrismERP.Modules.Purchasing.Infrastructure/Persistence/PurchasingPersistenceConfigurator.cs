using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;

namespace PrismERP.Modules.Purchasing.Infrastructure.Persistence;

internal sealed class PurchasingPersistenceConfigurator : IModulePersistenceConfigurator
{
    public void Configure(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PurchasingPersistenceConfigurator).Assembly);
}
