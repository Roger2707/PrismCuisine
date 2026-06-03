using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.SalesOrdering.Application.Abtractions;
using PrismCuisine.Modules.SalesOrdering.Domain.Entities;

namespace PrismCuisine.Modules.SalesOrdering.Infrastructure.Persistence.Repositories;
internal sealed class CustomerRepository(PrismCuisineDbContext db) : ICustomerRepository
{
    public async Task<IReadOnlyCollection<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.Customers
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await db.Customers.FirstOrDefaultAsync(c => c.Code == code.Trim().ToUpperInvariant(), cancellationToken);
    }

    public Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return db.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public void Add(Customer customer)
    {
        db.Add(customer);
    }

    public void Update(Customer customer)
    {
        db.Update(customer);
    }

    public async Task<bool> IsExists(int id) => await db.Customers.AnyAsync(c => c.Id == id);
}
