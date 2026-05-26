using Microsoft.EntityFrameworkCore;

namespace PrismCuisine.BuildingBlocks.Infrastructure.Persistence;

public interface IModulePersistenceConfigurator
{
    void Configure(ModelBuilder modelBuilder);
}
