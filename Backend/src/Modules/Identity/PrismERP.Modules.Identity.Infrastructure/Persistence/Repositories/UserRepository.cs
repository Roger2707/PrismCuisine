using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Identity.Application.Abstractions.Persistence;
using PrismERP.Modules.Identity.Domain.Entities;
using System.Threading;

namespace PrismERP.Modules.Identity.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(PrismERPDbContext db) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLowerInvariant(), cancellationToken);

    public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    public void Add(User user) => db.Users.Add(user);

    public void Update(User user) => db.Users.Update(user);
}
