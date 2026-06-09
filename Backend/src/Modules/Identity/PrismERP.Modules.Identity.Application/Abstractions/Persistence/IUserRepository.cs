using PrismERP.Modules.Identity.Domain.Entities;

namespace PrismERP.Modules.Identity.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);
    void Add(User user);
    void Update(User user);
}
