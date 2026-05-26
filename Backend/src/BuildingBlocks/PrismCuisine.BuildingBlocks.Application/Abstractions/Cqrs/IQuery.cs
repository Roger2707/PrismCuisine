using MediatR;

namespace PrismCuisine.BuildingBlocks.Application.Abstractions.Cqrs;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
