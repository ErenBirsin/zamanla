using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;
using Zamanla.Domain.SalonServices;
using Zamanla.Domain.SalonServices.ValueObjects;
using Zamanla.Domain.Shared;

namespace Zamanla.Application.SalonServices;

public sealed record SalonServiceUpdateCommand(
    Guid Id,
    string Name,
    string Category,
    int DurationMinutes,
    int BufferMinutes,
    decimal Price,
    bool IsActive) : IRequest<Result<string>>;

public sealed class SalonServiceUpdateCommandValidator : AbstractValidator<SalonServiceUpdateCommand>
{
    public SalonServiceUpdateCommandValidator()
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

internal sealed class SalonServiceUpdateCommandHandler(
    ISalonServiceRepository salonServiceRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SalonServiceUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(SalonServiceUpdateCommand request, CancellationToken cancellationToken)
    {
        var salonService = await salonServiceRepository.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (salonService is null)
        {
            return Result<string>.Failure(404, "Hizmet bulunamadı");
        }

        var name = request.Name.Trim();
        if (await salonServiceRepository.AnyAsync(s => s.Name.Value == name && s.Id != request.Id, cancellationToken))
        {
            return Result<string>.Failure(409, "Bu isimde bir hizmet zaten var");
        }

        salonService.SetName(new Name(name));
        salonService.SetCategory(new Category(request.Category.Trim()));
        salonService.SetDuration(new Duration(request.DurationMinutes));
        salonService.SetBuffer(new Duration(request.BufferMinutes));
        salonService.SetPrice(new Price(request.Price));

        if (request.IsActive)
        {
            salonService.Activate();
        }
        else
        {
            salonService.Deactivate();
        }

        salonServiceRepository.Update(salonService);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Hizmet güncellendi";
    }
}
