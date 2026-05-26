using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PrismCuisine.BuildingBlocks.Infrastructure.Persistence;

public sealed class DesignTimePrismCuisineDbContextFactory : IDesignTimeDbContextFactory<PrismCuisineDbContext>
{
    public PrismCuisineDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("PRISM_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=prism_cuisine;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<PrismCuisineDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new PrismCuisineDbContext(optionsBuilder.Options, []);
    }
}
