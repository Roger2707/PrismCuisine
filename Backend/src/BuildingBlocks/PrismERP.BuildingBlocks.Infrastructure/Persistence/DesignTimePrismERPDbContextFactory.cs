using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Reflection;

namespace PrismERP.BuildingBlocks.Infrastructure.Persistence;

public sealed class DesignTimePrismERPDbContextFactory : IDesignTimeDbContextFactory<PrismERPDbContext>
{
    public PrismERPDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("PRISM_DB_CONNECTION")
            ?? "Server=localhost\\ROGER;Database=PrismERP;Trusted_Connection=True;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<PrismERPDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        var basePath = AppDomain.CurrentDomain.BaseDirectory;

        var loadedAssemblies = Directory.GetFiles(basePath, "PrismERP.*.dll")
            .Select(Assembly.LoadFrom)
            .ToList();

        var configurators = loadedAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IModulePersistenceConfigurator).IsAssignableFrom(type)
                           && !type.IsInterface
                           && !type.IsAbstract)
            .Select(type => (IModulePersistenceConfigurator)Activator.CreateInstance(type)!)
            .ToList();

        return new PrismERPDbContext(optionsBuilder.Options, configurators);
    }
}
