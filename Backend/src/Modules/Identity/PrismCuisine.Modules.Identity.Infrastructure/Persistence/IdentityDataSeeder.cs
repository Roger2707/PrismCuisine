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
        var superAdminRole = await db.Roles
            .FirstOrDefaultAsync(x => x.NormalizedName == "SUPER_ADMIN", cancellationToken);

        if (superAdminRole is null)
        {
            superAdminRole = Role.Create("super_admin");
            db.Roles.Add(superAdminRole);
        }

        var user = await db.Users
            .FirstOrDefaultAsync(x => x.Email == "admin@prism.local", cancellationToken);

        if (user is null)
        {
            user = User.Register("admin@prism.local", "System Admin", passwordHasher.Hash("Admin@123"));
            db.Users.Add(user);
        }

        var existingUserRole = await db.UserRoles.AnyAsync(
            x => x.UserId == user.Id && x.RoleId == superAdminRole.Id,
            cancellationToken);

        if (!existingUserRole)
        {
            db.UserRoles.Add(UserRole.Create(user.Id, superAdminRole.Id));
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
