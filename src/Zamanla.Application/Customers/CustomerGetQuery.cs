using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Domain.Customers;

namespace Zamanla.Application.Customers;

public sealed record CustomerGetQuery(Guid Id) : IRequest<Result<CustomerDto>>;

internal sealed class CustomerGetQueryHandler(
    ICustomerRepository customerRepository) : IRequestHandler<CustomerGetQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(CustomerGetQuery request, CancellationToken cancellationToken)
    {
        var res = await customerRepository
            .GetAllWithAudit()
            .MapTo()
            .Where(c => c.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (res is null)
        {
            return Result<CustomerDto>.Failure(404, "Müşteri bulunamadı");
        }

        return res;
    }
}
