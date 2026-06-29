using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Identity.Domain.Entities;
using PrismERP.Modules.Identity.Infrastructure.Auth;

namespace PrismERP.Modules.Identity.Infrastructure.Persistence;

public interface IIdentityDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

internal sealed class IdentityDataSeeder(PrismERPDbContext db, Pbkdf2PasswordHasher passwordHasher) : IIdentityDataSeeder
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
            // Identity
            new { Code = "users:read", Description = "View user profiles" },
            new { Code = "roles:read", Description = "View roles and permission catalog" },
            new { Code = "roles:write", Description = "Manage roles and permission assignments" },

            // Inventory — master data
            new { Code = "product-category:read", Description = "View product categories" },
            new { Code = "product-category:write", Description = "Create and update product categories" },
            new { Code = "product:read", Description = "View products and SKU catalog" },
            new { Code = "product:write", Description = "Create, update, and deactivate products" },
            new { Code = "warehouse:read", Description = "View warehouses" },
            new { Code = "warehouse:write", Description = "Create and update warehouses" },
            new { Code = "supplier:read", Description = "View suppliers" },
            new { Code = "supplier:write", Description = "Create, update, and deactivate suppliers" },
            new { Code = "customer:read", Description = "View customers" },
            new { Code = "customer:write", Description = "Create, update, and deactivate customers" },

            // Inventory — stock operations
            new { Code = "inventory:read", Description = "View balances, movements, reservations, and cost layers" },
            new { Code = "inventory:adjust", Description = "Receive, issue, adjust stock, and manage reservations" },

            // Purchasing — PO / GR / AP invoice
            new { Code = "purchase:read", Description = "View purchase orders" },
            new { Code = "purchase:write", Description = "Create and update draft purchase orders" },
            new { Code = "purchase:approve", Description = "Approve purchase orders for receiving" },
            new { Code = "purchase:amend", Description = "Amend approved purchase orders" },
            new { Code = "purchase:cancel", Description = "Cancel draft or approved purchase orders" },
            new { Code = "goods-receipt:read", Description = "View goods receipts" },
            new { Code = "goods-receipt:write", Description = "Create and update draft goods receipts" },
            new { Code = "goods-receipt:post", Description = "Post goods receipts into inventory" },
            new { Code = "goods-receipt:cancel", Description = "Cancel posted goods receipts" },
            new { Code = "purchase-invoice:read", Description = "View purchase invoices" },
            new { Code = "purchase-invoice:write", Description = "Create purchase invoices from goods receipts" },

            // Sales — SO / DN
            new { Code = "salesorder:read", Description = "View sales orders" },
            new { Code = "salesorder:write", Description = "Create and update draft sales orders" },
            new { Code = "salesorder:approve", Description = "Approve sales orders and reserve inventory" },
            new { Code = "salesorder:cancel", Description = "Cancel draft or confirmed sales orders" },
            new { Code = "delivery:read", Description = "View delivery notes" },
            new { Code = "delivery:write", Description = "Create and update draft delivery notes" },
            new { Code = "delivery:post", Description = "Post delivery notes and issue inventory" },
            new { Code = "delivery:cancel", Description = "Cancel posted delivery notes" },

            // Finance — AR invoice & payments
            new { Code = "invoice:read", Description = "View sales invoices" },
            new { Code = "invoice:write", Description = "Create and cancel sales invoices" },
            new { Code = "payment:read", Description = "View payments" },
            new { Code = "payment:write", Description = "Create and update payments" },
            new { Code = "payment:process", Description = "Complete, fail, cancel, or refund payments" }
        };

        foreach (var config in permissionConfig)
        {
            var normalizedCode = config.Code.Trim().ToLowerInvariant();
            var existing = await db.Permissions
                .FirstOrDefaultAsync(x => x.Code == normalizedCode, cancellationToken);

            if (existing is null)
            {
                db.Permissions.Add(Permission.Create(config.Code, config.Description));
                continue;
            }

            if (!string.Equals(existing.Description, config.Description, StringComparison.Ordinal))
            {
                existing.Update(config.Code, config.Description);
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
        var allRoles = await db.Roles.ToListAsync(cancellationToken);
        var allPermissions = await db.Permissions.ToListAsync(cancellationToken);

        var adminRole = allRoles.First(r => r.NormalizedName == "SUPER_ADMIN");
        var managerRole = allRoles.First(r => r.NormalizedName == "MANAGER");
        var leaderRole = allRoles.First(r => r.NormalizedName == "LEADER");
        var staffRole = allRoles.First(r => r.NormalizedName == "STAFF");

        var rolePermissionMapping = new Dictionary<int, List<string>>();

        // Staff — day-to-day data entry and posting (no approvals / cancel rollback)
        var staffPermissions = new List<string>
        {
            "product-category:read", "product:read", "warehouse:read",
            "supplier:read", "customer:read", "inventory:read",
            "purchase:read", "purchase:write",
            "goods-receipt:read", "goods-receipt:write", "goods-receipt:post",
            "purchase-invoice:read",
            "salesorder:read", "salesorder:write",
            "delivery:read", "delivery:write", "delivery:post",
            "invoice:read", "payment:read"
        };
        rolePermissionMapping.Add(staffRole.Id, staffPermissions);

        // Leader — staff + SO approval
        var leaderPermissions = new List<string>(staffPermissions)
        {
            "salesorder:approve"
        };
        rolePermissionMapping.Add(leaderRole.Id, leaderPermissions);

        // Manager — full operational control (except identity admin)
        var managerPermissions = new List<string>
        {
            "product-category:read", "product-category:write",
            "product:read", "product:write",
            "warehouse:read", "warehouse:write",
            "supplier:read", "supplier:write",
            "customer:read", "customer:write",
            "inventory:read", "inventory:adjust",
            "purchase:read", "purchase:write", "purchase:approve", "purchase:amend", "purchase:cancel",
            "goods-receipt:read", "goods-receipt:write", "goods-receipt:post", "goods-receipt:cancel",
            "purchase-invoice:read", "purchase-invoice:write",
            "salesorder:read", "salesorder:write", "salesorder:approve", "salesorder:cancel",
            "delivery:read", "delivery:write", "delivery:post", "delivery:cancel",
            "invoice:read", "invoice:write",
            "payment:read", "payment:write", "payment:process"
        };
        rolePermissionMapping.Add(managerRole.Id, managerPermissions);

        // Super admin — all permissions (also bypassed by IsSuperAdminAsync)
        rolePermissionMapping.Add(adminRole.Id, allPermissions.Select(p => p.Code).ToList());

        foreach (var mapping in rolePermissionMapping)
        {
            var roleId = mapping.Key;
            var allowedCodes = mapping.Value.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var targetPermissions = allPermissions.Where(p => allowedCodes.Contains(p.Code));

            foreach (var permission in targetPermissions)
            {
                var exists = await db.RolePermissions.AnyAsync(
                    rp => rp.RoleId == roleId && rp.PermissionId == permission.Id,
                    cancellationToken);

                if (!exists)
                {
                    db.RolePermissions.Add(RolePermission.Create(roleId, permission.Id));
                }
            }
        }
    }
}
