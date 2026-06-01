using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Identity.Domain.Entities;
using PrismCuisine.Modules.Identity.Infrastructure.Auth;

namespace PrismCuisine.Modules.Identity.Infrastructure.Persistence;

public interface IIdentityDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

internal sealed class IdentityDataSeeder(PrismCuisineDbContext db, Pbkdf2PasswordHasher passwordHasher) : IIdentityDataSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedPermissionsAsync(cancellationToken);
        await SeedRolesAndUsersAsync(cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        await SeedRolePermissionsAsync(cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedPermissionsAsync(CancellationToken cancellationToken)
    {
        var permissionConfig = new[]
        {
        // ==========================================
        // CORE USER & ROLE MANAGEMENT
        // ==========================================
        new { Code = "users:read", Description = "View users list and details" },
        new { Code = "users:write", Description = "Create, update, and manage users" },
        new { Code = "roles:read", Description = "View roles and permissions" },
        new { Code = "roles:write", Description = "Manage roles and assign permissions" },
        
        // ==========================================
        // INVENTORY & MASTER DATA
        // ==========================================
        new { Code = "product:read", Description = "View product catalog and inventory levels" },
        new { Code = "product:write", Description = "Create and update product master data" },

        new { Code = "warehouse:read", Description = "View warehouse locations and stock movements" },
        new { Code = "warehouse:write", Description = "Manage warehouse configurations and stock adjustments" },
        
        // ==========================================
        // SUPPLY CHAIN & TRANSACTIONAL WORKFLOWS
        // ==========================================
        
        // Purchase Orders
        new { Code = "purchase:read", Description = "View purchase orders" },
        new { Code = "purchase:create", Description = "Create draft purchase orders" },
        new { Code = "purchase:approve", Description = "Approve and authorize purchase orders" },
        new { Code = "purchase:amend", Description = "Amend/Modify finalized or sent purchase orders" },
        
        // Receipts (Goods Receipt Note)
        new { Code = "receipt:read", Description = "View goods receipt notes" },
        new { Code = "receipt:process", Description = "Process and confirm incoming shipments from vendors" },
        
        // Sales Orders
        new { Code = "salesorder:read", Description = "View sales orders" },
        new { Code = "salesorder:create", Description = "Create and update standard sales orders" },
        new { Code = "salesorder:approve", Description = "Approve credit limits and release sales orders for fulfillment" },
        
        // Delivery (Goods Issue / Outbound)
        new { Code = "delivery:read", Description = "View delivery orders and shipments" },
        new { Code = "delivery:process", Description = "Pick, pack, and confirm outbound deliveries" },
        
        // Invoices
        new { Code = "invoice:read", Description = "View billing and invoices" },
        new { Code = "invoice:create", Description = "Generate standard customer invoices" },
        new { Code = "invoice:approve", Description = "Post invoices to accounting" },
        new { Code = "invoice:void", Description = "Void or issue credit notes for finalized invoices" }
    };

        foreach (var config in permissionConfig)
        {
            var exists = await db.Permissions
                .AnyAsync(x => x.Code == config.Code, cancellationToken);

            if (!exists)
            {
                var permission = Permission.Create(config.Code, config.Description);
                db.Permissions.Add(permission);
            }
        }
    }

    public async Task SeedRolesAndUsersAsync(CancellationToken cancellationToken = default)
    {
        var seedConfig = new[]
        {
            new { Role = "super_admin", Prefix = "admin", Name = "System Admin", Count = 1 },
            new { Role = "manager", Prefix = "manager", Name = "Manager", Count = 3 },
            new { Role = "leader", Prefix = "leader", Name = "Leader", Count = 3 },
            new { Role = "staff", Prefix = "staff", Name = "Staff", Count = 3 }
        };

        foreach (var config in seedConfig)
        {
            var role = await db.Roles
                .FirstOrDefaultAsync(x => x.NormalizedName == config.Role.ToUpper(), cancellationToken);

            if (role is null)
            {
                role = Role.Create(config.Role);
                db.Roles.Add(role);
                await db.SaveChangesAsync(cancellationToken);
            }

            for (int i = 1; i <= config.Count; i++)
            {
                var email = config.Count == 1 ? $"{config.Prefix}@prism.local" : $"{config.Prefix}{i}@prism.local";
                var fullName = config.Count == 1 ? config.Name : $"{config.Name} {i}";
                var defaultPassword = config.Count == 1 ? "Admin@123" : $"{config.Name}@123";

                var user = await db.Users
                    .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

                if (user is null)
                {
                    user = User.Register(email, fullName, passwordHasher.Hash(defaultPassword));
                    db.Users.Add(user);
                    await db.SaveChangesAsync(cancellationToken);
                }

                var existingUserRole = await db.UserRoles.AnyAsync(
                    x => x.UserId == user.Id && x.RoleId == role.Id,
                    cancellationToken);

                if (!existingUserRole)
                {
                    db.UserRoles.Add(UserRole.Create(user.Id, role.Id));
                    await db.SaveChangesAsync(cancellationToken);
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedRolePermissionsAsync(CancellationToken cancellationToken)
    {
        // Look up all Roles and Permissions from DB to get their real IDs
        var allRoles = await db.Roles.ToListAsync(cancellationToken);
        var allPermissions = await db.Permissions.ToListAsync(cancellationToken);

        var adminRole = allRoles.First(r => r.NormalizedName == "SUPER_ADMIN");
        var managerRole = allRoles.First(r => r.NormalizedName == "MANAGER");
        var leaderRole = allRoles.First(r => r.NormalizedName == "LEADER");
        var staffRole = allRoles.First(r => r.NormalizedName == "STAFF");

        // Define which permission codes belong to which role
        var rolePermissionMapping = new Dictionary<int, List<string>>();

        // -------------------------------------------------------------
        // 1. Staff Permissions (Execution Only)
        // -------------------------------------------------------------
        var staffPermissions = new List<string>
    {
        "product:read", "warehouse:read",
        "purchase:read", "purchase:create",
        "receipt:read", "receipt:process",
        "salesorder:read", "salesorder:create",
        "delivery:read", "delivery:process",
        "invoice:read"
    };
        rolePermissionMapping.Add(staffRole.Id, staffPermissions);

        // -------------------------------------------------------------
        // 2. Leader Permissions (Execution + Basic Approvals)
        // -------------------------------------------------------------
        var leaderPermissions = new List<string>(staffPermissions) // Inherits all Staff permissions
    {
        "salesorder:approve" // Leaders can release sales orders for shipment
    };
        rolePermissionMapping.Add(leaderRole.Id, leaderPermissions);

        // -------------------------------------------------------------
        // 3. Manager Permissions (Full Operational Control)
        // -------------------------------------------------------------
        var managerPermissions = new List<string>
    {
        "product:read", "product:write",
        "warehouse:read", "warehouse:write",
        "purchase:read", "purchase:create", "purchase:approve", "purchase:amend",
        "receipt:read", "receipt:process",
        "salesorder:read", "salesorder:create", "salesorder:approve",
        "delivery:read", "delivery:process",
        "invoice:read", "invoice:create", "invoice:approve", "invoice:void"
    };
        rolePermissionMapping.Add(managerRole.Id, managerPermissions);

        // -------------------------------------------------------------
        // 4. Super Admin Permissions (Everything)
        // -------------------------------------------------------------
        var adminPermissions = allPermissions.Select(p => p.Code).ToList();
        rolePermissionMapping.Add(adminRole.Id, adminPermissions);

        // -------------------------------------------------------------
        // Process and Seed to DB
        // -------------------------------------------------------------
        foreach (var mapping in rolePermissionMapping)
        {
            var roleId = mapping.Key;
            var allowedCodes = mapping.Value;

            // Filter the actual permission entities matching the codes allowed for this role
            var targetPermissions = allPermissions.Where(p => allowedCodes.Contains(p.Code));

            foreach (var permission in targetPermissions)
            {
                // Avoid duplicates check
                var exists = await db.RolePermissions.AnyAsync(
                    rp => rp.RoleId == roleId && rp.PermissionId == permission.Id,
                    cancellationToken);

                if (!exists)
                {
                    // Adjust this line to match your exact RolePermission constructor / factory method
                    var rolePermission = RolePermission.Create(roleId, permission.Id);
                    db.RolePermissions.Add(rolePermission);
                }
            }
        }
    }
}
