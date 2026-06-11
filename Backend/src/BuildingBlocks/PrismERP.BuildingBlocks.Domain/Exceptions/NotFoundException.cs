namespace PrismERP.BuildingBlocks.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string resource, object id) : base($"{resource} with id '{id}' was not found.") { }

    public NotFoundException(string message) : base(message) { }
}
