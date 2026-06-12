using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Finance.Application.Abstractions.Persistence;

namespace PrismERP.Modules.Finance.Infrastructure.Persistence;

internal interface IFinanceDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

internal sealed class FinanceDataSeeder(IFinanceUnitOfWork unitOfWork) : IFinanceDataSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Add sample data if needed
        // For now, this is a placeholder for future data seeding logic
        await Task.CompletedTask;
    }
}
