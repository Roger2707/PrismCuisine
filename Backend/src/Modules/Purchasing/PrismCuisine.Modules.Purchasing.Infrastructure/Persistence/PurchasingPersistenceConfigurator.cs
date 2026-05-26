using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;

namespace PrismCuisine.Modules.Purchasing.Infrastructure.Persistence;

internal sealed class PurchasingPersistenceConfigurator : IModulePersistenceConfigurator
{
    public void Configure(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PurchasingPersistenceConfigurator).Assembly);
}
