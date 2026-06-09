using Microsoft.EntityFrameworkCore;

namespace PrismERP.BuildingBlocks.Infrastructure.Persistence;

public interface IModulePersistenceConfigurator
{
    void Configure(ModelBuilder modelBuilder);
}
