using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;
using Zamanla.Domain.Customers;
using Zamanla.Domain.Customers.ValueObjects;
using Zamanla.Domain.Shared;

namespace Zamanla.Application.Customers;

public sealed record CustomerCreateCommand(
    string FirstName,
    string LastName,
    string PhoneNumber,
    string? Email,
    string? Note) : IRequest<Result<string>>;

public sealed class CustomerCreateCommandValidator : AbstractValidator<CustomerCreateCommand>
{
    public CustomerCreateCommandValidator()
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

internal sealed class CustomerCreateCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CustomerCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CustomerCreateCommand request, CancellationToken cancellationToken)
    {
        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        PhoneNumber phoneNumber = new(request.PhoneNumber);

        // aynı telefon farklı kişilerde olabilir (kardeş abi) aynı kişi iki kez eklenmesin
        var exists = await customerRepository.AnyAsync(c =>
            !c.IsAnonymized &&
            c.PhoneNumber!.Value == phoneNumber.Value &&
            c.FirstName.Value == firstName &&
            c.LastName.Value == lastName,
            cancellationToken);
        if (exists)
        {
            return Result<string>.Failure(409, "Bu ad ve telefonla kayıtlı bir müşteri var");
        }

        Customer customer = new(
            new FirstName(firstName),
            new LastName(lastName),
            phoneNumber,
            string.IsNullOrWhiteSpace(request.Email) ? null : new Email(request.Email),
            string.IsNullOrWhiteSpace(request.Note) ? null : new Note(request.Note.Trim()));

        customerRepository.Add(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Müşteri kaydedildi";
    }
}
