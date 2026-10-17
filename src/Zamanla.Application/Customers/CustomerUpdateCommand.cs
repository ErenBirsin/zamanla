using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;
using Zamanla.Domain.Customers;
using Zamanla.Domain.Shared;

namespace Zamanla.Application.Customers;

public sealed record CustomerUpdateCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string? Email,
    string? Note) : IRequest<Result<string>>;

public sealed class CustomerUpdateCommandValidator : AbstractValidator<CustomerUpdateCommand>
{
    public CustomerUpdateCommandValidator()
    {
        RuleFor(p => p.FirstName)
            .NotEmpty().WithMessage("Geçerli bir ad girin")
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir");
        RuleFor(p => p.LastName)
            .NotEmpty().WithMessage("Geçerli bir soyad girin")
            .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir");
        RuleFor(p => p.PhoneNumber)
            .NotEmpty().WithMessage("Geçerli bir telefon numarası girin")
            .Must(PhoneNumber.IsValid).WithMessage("Geçerli bir telefon numarası girin");
        RuleFor(p => p.Email)
            .EmailAddress().WithMessage("Geçerli bir mail adresi girin")
            .MaximumLength(254).WithMessage("Mail adresi en fazla 254 karakter olabilir")
            .When(p => !string.IsNullOrWhiteSpace(p.Email));
        RuleFor(p => p.Note)
            .MaximumLength(500).WithMessage("Not en fazla 500 karakter olabilir");
    }
}

internal sealed class CustomerUpdateCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CustomerUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CustomerUpdateCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.FirstOrDefaultAsync(
            c => c.Id == request.Id && !c.IsAnonymized,
            cancellationToken);
        if (customer is null)
        {
            return Result<string>.Failure(404, "Müşteri bulunamadı");
        }

        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        PhoneNumber phoneNumber = new(request.PhoneNumber);

        var exists = await customerRepository.AnyAsync(c =>
            c.Id != request.Id &&
            !c.IsAnonymized &&
            c.PhoneNumber!.Value == phoneNumber.Value &&
            c.FirstName.Value == firstName &&
            c.LastName.Value == lastName,
            cancellationToken);
        if (exists)
        {
            return Result<string>.Failure(409, "Bu ad ve telefonla kayıtlı bir müşteri var");
        }

        customer.SetFirstName(new FirstName(firstName));
        customer.SetLastName(new LastName(lastName));
        customer.SetPhoneNumber(phoneNumber);
        customer.SetEmail(string.IsNullOrWhiteSpace(request.Email) ? null : new Email(request.Email));
        customer.SetNote(string.IsNullOrWhiteSpace(request.Note) ? null : new Note(request.Note.Trim()));

        customerRepository.Update(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Müşteri güncellendi";
    }
}

