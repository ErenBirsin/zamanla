using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;
using Zamanla.Domain.SalonServices;
using Zamanla.Domain.SalonServices.ValueObjects;
using Zamanla.Domain.Shared;
using Zamanla.Application.Behaviors;
using Zamanla.Application.Permissions;

namespace Zamanla.Application.SalonServices;

[Permission(PermissionCatalog.ServicesManage)]
public sealed record SalonServiceCreateCommand(
    string Name,
    string Category,
    int DurationMinutes,
    int BufferMinutes,
    decimal Price) : IRequest<Result<string>>;

public sealed class SalonServiceCreateCommandValidator : AbstractValidator<SalonServiceCreateCommand>
{
    public SalonServiceCreateCommandValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Geçerli bir hizmet adı girin")
            .MaximumLength(100).WithMessage("Hizmet adı en fazla 100 karakter olabilir");
        RuleFor(p => p.Category)
            .NotEmpty().WithMessage("Geçerli bir kategori girin")
            .MaximumLength(100).WithMessage("Kategori en fazla 100 karakter olabilir");
        RuleFor(p => p.DurationMinutes)
            .InclusiveBetween(5, 480).WithMessage("Süre 5 ile 480 dakika arasında olmalı");
        RuleFor(p => p.BufferMinutes)
            .InclusiveBetween(0, 120).WithMessage("Hazırlık payı 0 ile 120 dakika arasında olmalı");
        RuleFor(p => p.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Fiyat negatif olamaz");
    }
}

internal sealed class SalonServiceCreateCommandHandler(
    ISalonServiceRepository salonServiceRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SalonServiceCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(SalonServiceCreateCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (await salonServiceRepository.AnyAsync(s => s.Name.Value == name, cancellationToken))
        {
            return Result<string>.Failure(409, "Bu isimde bir hizmet zaten var");
        }

        SalonService salonService = new(
            new Name(name),
            new Category(request.Category.Trim()),
            new Duration(request.DurationMinutes),
            new Duration(request.BufferMinutes),
            new Price(request.Price));

        salonServiceRepository.Add(salonService);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Hizmet kaydedildi";
    }
}
