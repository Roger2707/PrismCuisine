using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;

namespace PrismERP.Modules.Finance.Infrastructure.Persistence;

internal sealed class FinancePersistenceConfigurator : IModulePersistenceConfigurator
{
    public void Configure(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinancePersistenceConfigurator).Assembly);
}
