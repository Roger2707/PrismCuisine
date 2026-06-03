
using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.SalesOrdering.Application.Abtractions;
using PrismCuisine.Modules.SalesOrdering.Domain.Entities;

namespace PrismCuisine.Modules.SalesOrdering.Application.Customers
{
    public sealed class CustomerService(ISalesOrderingUnitOfWork unitOfWork) : ICustomerService
    {
        public async Task<IReadOnlyCollection<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var customers = await unitOfWork.Customers.GetAllAsync(cancellationToken);
            return customers.Select(Map).ToList();
        }

        public async Task<CustomerDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var customer = await unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
            return customer is not null ? Map(customer) : null;
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
        {
            var customer = Customer.Create(
                request.Code,
                request.Name,
                request.Phone,
                request.Email,
                request.Address,
                request.TaxCode);
            unitOfWork.Customers.Add(customer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(customer);
        }

        public async Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
        {
            var customer = await unitOfWork.Customers.GetByIdAsync(id, cancellationToken)
                ?? throw new DomainException($"Customer '{id}' was not found.");

            customer.Deactivate();
            unitOfWork.Customers.Update(customer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
        {
            var customer = await unitOfWork.Customers.GetByIdAsync(id, cancellationToken)
                ?? throw new DomainException($"Customer '{id}' was not found.");

            unitOfWork.Customers.Update(customer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        #region Helpers

        private static CustomerDto Map(Customer customer) =>
        new(
            customer.Id,
            customer.Code,
            customer.Name,
            customer.Phone,
            customer.Email,
            customer.Address,
            customer.TaxCode,
            customer.IsActive);

        #endregion
    }
}
