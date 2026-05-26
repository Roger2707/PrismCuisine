using MediatR;

namespace PrismCuisine.BuildingBlocks.Application.Abstractions.Cqrs;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
