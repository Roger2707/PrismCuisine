using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;

namespace PrismCuisine.Modules.Identity.Infrastructure.Persistence;

internal sealed class IdentityPersistenceConfigurator : IModulePersistenceConfigurator
{
    public void Configure(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityPersistenceConfigurator).Assembly);
}
