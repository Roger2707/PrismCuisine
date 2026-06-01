using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.Purchasing.Application.Abstractions;
using PrismCuisine.Modules.Purchasing.Domain.Entities;

namespace PrismCuisine.Modules.Purchasing.Application.Suppliers;

public sealed class SupplierService(IPurchasingUnitOfWork unitOfWork) : ISupplierService
{
    public async Task<IReadOnlyCollection<SupplierDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var suppliers = await unitOfWork.Suppliers.GetAllAsync(cancellationToken);
        return suppliers.Select(Map).ToList();
    }

    public async Task<SupplierDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var supplier = await unitOfWork.Suppliers.GetByIdAsync(id, cancellationToken);
        return supplier is null ? null : Map(supplier);
    }

    public async Task<SupplierDto> CreateAsync(
        CreateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await unitOfWork.Suppliers.GetByCodeAsync(request.Code, cancellationToken);
        if (existing is not null)
        {
            throw new DomainException($"Supplier code '{request.Code}' already exists.");
        }

        var supplier = Supplier.Create(
            request.Code,
            request.Name,
            request.Phone,
            request.Email,
            request.Address,
            request.TaxCode);

        unitOfWork.Suppliers.Add(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(supplier);
    }

    public async Task UpdateAsync(
        int id,
        UpdateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        var supplier = await unitOfWork.Suppliers.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException($"Supplier '{id}' was not found.");

        supplier.Update(request.Name, request.Phone, request.Email, request.Address, request.TaxCode);
        unitOfWork.Suppliers.Update(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var supplier = await unitOfWork.Suppliers.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException($"Supplier '{id}' was not found.");

        supplier.Deactivate();
        unitOfWork.Suppliers.Update(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static SupplierDto Map(Supplier supplier) =>
        new(
            supplier.Id,
            supplier.Code,
            supplier.Name,
            supplier.Phone,
            supplier.Email,
            supplier.Address,
            supplier.TaxCode,
            supplier.IsActive);
}
