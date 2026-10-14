using GenericRepository;
using TS.MediatR;
using TS.Result;
using Zamanla.Domain.Customers;

namespace Zamanla.Application.Customers;

public sealed record CustomerDeleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class CustomerDeleteCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CustomerDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CustomerDeleteCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.FirstOrDefaultAsync(
            c => c.Id == request.Id && !c.IsAnonymized,
            cancellationToken);
        if (customer is null)
        {
            return Result<string>.Failure(404, "Müşteri bulunamadı");
        }

        // kayıt silinmez, kişisel bilgiler temizlenir 
        customer.Anonymize();
        customerRepository.Update(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Müşteri silindi";
    }
}
