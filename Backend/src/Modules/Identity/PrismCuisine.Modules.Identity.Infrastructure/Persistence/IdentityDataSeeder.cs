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
            }

            for (int i = 1; i <= config.Count; i++)
            {
                // Định dạng Email, Tên hiển thị và Mật khẩu theo chuẩn chung
                // Nếu Count = 1 (mục admin) -> Email là admin@prism.local, Tên là System Admin
                // Nếu Count > 1 -> Email là manager1@prism.local, Tên là Manager 1
                var email = config.Count == 1 ? $"{config.Prefix}@prism.local" : $"{config.Prefix}{i}@prism.local";
                var fullName = config.Count == 1 ? config.Name : $"{config.Name} {i}";
                var defaultPassword = config.Count == 1 ? "Admin@123" : $"{config.Name}@123";

                var user = await db.Users
                    .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

                if (user is null)
                {
                    user = User.Register(email, fullName, passwordHasher.Hash(defaultPassword));
                    db.Users.Add(user);
                }

                var existingUserRole = await db.UserRoles.AnyAsync(
                    x => x.UserId == user.Id && x.RoleId == role.Id,
                    cancellationToken);

                if (!existingUserRole)
                {
                    db.UserRoles.Add(UserRole.Create(user.Id, role.Id));
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
