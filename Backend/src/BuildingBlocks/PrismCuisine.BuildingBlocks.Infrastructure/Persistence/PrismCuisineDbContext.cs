using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Domain.Aggregates;
using PrismCuisine.BuildingBlocks.Domain.Entities;
using PrismCuisine.Modules.Identity.Domain.Entities;
using PrismCuisine.Modules.Inventory.Domain.Entities;
using PrismCuisine.Modules.Purchasing.Domain.Entities;
using SalesOrderEntity = PrismCuisine.Modules.SalesOrder.Domain.Entities.SalesOrder;
using SalesOrderLineEntity = PrismCuisine.Modules.SalesOrder.Domain.Entities.SalesOrderLine;

namespace PrismCuisine.BuildingBlocks.Infrastructure.Persistence;

public sealed class PrismCuisineDbContext : DbContext
{
    private readonly IEnumerable<IModulePersistenceConfigurator> _moduleConfigurators;

    public PrismCuisineDbContext(
        DbContextOptions<PrismCuisineDbContext> options,
        IEnumerable<IModulePersistenceConfigurator> moduleConfigurators)
        : base(options)
    {
        _moduleConfigurators = moduleConfigurators;
    }

    // Identity
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Inventory
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<InventoryBalance> InventoryBalances => Set<InventoryBalance>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<InventoryCostLayer> InventoryCostLayers => Set<InventoryCostLayer>();
    public DbSet<InventoryReservation> InventoryReservations => Set<InventoryReservation>();

    // Purchasing
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();

    // SalesOrder
    public DbSet<SalesOrderEntity> SalesOrders => Set<SalesOrderEntity>();
    public DbSet<SalesOrderLineEntity> SalesOrderLines => Set<SalesOrderLineEntity>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(Entity.UpdatedAt)).CurrentValue = DateTime.UtcNow;
            }
        }

        var aggregates = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var configurator in _moduleConfigurators)
        {
            configurator.Configure(modelBuilder);
        }

        base.OnModelCreating(modelBuilder);
    }
}
