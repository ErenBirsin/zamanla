using TS.MediatR;
using Zamanla.Domain.Customers;

namespace Zamanla.Application.Customers;

public sealed record CustomerGetAllQuery : IRequest<IQueryable<CustomerDto>>;

internal sealed class CustomerGetAllQueryHandler(
    ICustomerRepository customerRepository) : IRequestHandler<CustomerGetAllQuery, IQueryable<CustomerDto>>
{
    public Task<IQueryable<CustomerDto>> Handle(CustomerGetAllQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(customerRepository.GetAllWithAudit().MapTo());
}
