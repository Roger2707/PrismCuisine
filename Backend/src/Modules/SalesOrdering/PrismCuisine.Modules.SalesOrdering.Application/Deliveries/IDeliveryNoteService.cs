namespace PrismCuisine.Modules.SalesOrdering.Application.Deliveries;

public interface IDeliveryNoteService
{
    Task<IReadOnlyCollection<DeliveryNoteSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DeliveryNoteDto?> GetByIdAsync(int deliveryNoteId, CancellationToken cancellationToken = default);
    Task<DeliveryNoteDto> CreateAsync(CreateDeliveryNoteRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int deliveryNoteId, UpdateDeliveryNoteRequest request, CancellationToken cancellationToken = default);
    Task PostAsync(int deliveryNoteId, CancellationToken cancellationToken = default);
    Task CancelAsync(int deliveryNoteId, CancellationToken cancellationToken = default);
}
