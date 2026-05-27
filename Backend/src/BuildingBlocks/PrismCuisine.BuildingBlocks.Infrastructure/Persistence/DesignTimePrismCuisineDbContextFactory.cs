using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Reflection;

namespace PrismCuisine.BuildingBlocks.Infrastructure.Persistence;

public sealed class DesignTimePrismCuisineDbContextFactory : IDesignTimeDbContextFactory<PrismCuisineDbContext>
{
    public PrismCuisineDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("PRISM_DB_CONNECTION")
            ?? "Server=localhost\\ROGER;Database=prism_cuisine;Trusted_Connection=True;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<PrismCuisineDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        var basePath = AppDomain.CurrentDomain.BaseDirectory;

        var loadedAssemblies = Directory.GetFiles(basePath, "PrismCuisine.*.dll")
            .Select(Assembly.LoadFrom)
            .ToList();

        var configurators = loadedAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IModulePersistenceConfigurator).IsAssignableFrom(type)
                           && !type.IsInterface
                           && !type.IsAbstract)
            .Select(type => (IModulePersistenceConfigurator)Activator.CreateInstance(type)!)
            .ToList();

        return new PrismCuisineDbContext(optionsBuilder.Options, configurators);
    }
}
