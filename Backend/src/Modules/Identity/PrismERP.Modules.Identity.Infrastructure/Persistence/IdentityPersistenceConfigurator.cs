using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;

namespace PrismERP.Modules.Identity.Infrastructure.Persistence;

internal sealed class IdentityPersistenceConfigurator : IModulePersistenceConfigurator
{
    public void Configure(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityPersistenceConfigurator).Assembly);
}
