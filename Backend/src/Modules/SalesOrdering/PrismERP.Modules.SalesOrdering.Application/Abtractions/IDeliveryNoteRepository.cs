using PrismERP.Modules.SalesOrdering.Application.Deliveries;
using PrismERP.Modules.SalesOrdering.Domain.Entities;

namespace PrismERP.Modules.SalesOrdering.Application.Abtractions;

public interface IDeliveryNoteRepository
{
    Task<DeliveryNote?> GetByIdWithLinesForUpdateAsync(int id, CancellationToken cancellationToken = default);
    Task<List<DeliveryNote>> GetBySalesOrderIdAsync(int salesOrderId, CancellationToken cancellationToken = default);
    Task<DeliveryNoteDto?> GetByIdWithLinesAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DeliveryNoteSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<int> GetCountForDateAsync(DateTime date, CancellationToken cancellationToken = default);
    void Add(DeliveryNote deliveryNote);
    void Update(DeliveryNote deliveryNote);
    void Delete(DeliveryNote deliveryNote);
    void RemoveRange(List<DeliveryNote> deliveryNotes);
}
