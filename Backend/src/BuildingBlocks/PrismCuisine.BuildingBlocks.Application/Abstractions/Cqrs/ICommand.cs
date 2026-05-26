using MediatR;

namespace PrismCuisine.BuildingBlocks.Application.Abstractions.Cqrs;

public interface ICommand : IRequest
{
}

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
