using PrismERP.Modules.SalesOrdering.Application.Deliveries;
using PrismERP.Modules.SalesOrdering.Domain.Entities;

namespace PrismERP.Modules.SalesOrdering.Application.Abtractions;

public interface IDeliveryNoteRepository
{
    Task<DeliveryNote?> GetByIdWithLinesForUpdateAsync(int id, CancellationToken cancellationToken = default);
    Task<DeliveryNoteDto?> GetByIdWithLinesAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DeliveryNoteSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<int> GetCountForDateAsync(DateTime date, CancellationToken cancellationToken = default);
    void Add(DeliveryNote order);
    void Update(DeliveryNote order);
}
