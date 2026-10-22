using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Behaviors;
using Zamanla.Application.Permissions;
using Zamanla.Application.Services;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Tenants;
using Zamanla.Domain.Tenants.ValueObjects;

namespace Zamanla.Application.Settings;

[Permission(PermissionCatalog.SettingsManage)]
// slug bilerek yok: müşterilere paylaşılmış linkler bozulmasın
public sealed record SettingsUpdateCommand(
    string Name,
    Address? Address,
    string? PhoneNumber,
    BookingPolicy BookingPolicy) : IRequest<Result<string>>;

public sealed class SettingsUpdateCommandValidator : AbstractValidator<SettingsUpdateCommand>
{
    public SettingsUpdateCommandValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Geçerli bir salon adı girin")
            .MaximumLength(100).WithMessage("Salon adı en fazla 100 karakter olabilir");

        RuleFor(p => p.Address!.City)
            .NotEmpty().WithMessage("İl girin")
            .MaximumLength(50).WithMessage("İl en fazla 50 karakter olabilir")
            .When(p => p.Address is not null);
        RuleFor(p => p.Address!.District)
            .NotEmpty().WithMessage("İlçe girin")
            .MaximumLength(50).WithMessage("İlçe en fazla 50 karakter olabilir")
            .When(p => p.Address is not null);
        RuleFor(p => p.Address!.FullAddress)
            .NotEmpty().WithMessage("Açık adres girin")
            .MaximumLength(250).WithMessage("Açık adres en fazla 250 karakter olabilir")
            .When(p => p.Address is not null);

        RuleFor(p => p.PhoneNumber)
            .Must(phone => PhoneNumber.IsValid(phone!)).WithMessage("Geçerli bir telefon numarası girin")
            .When(p => !string.IsNullOrWhiteSpace(p.PhoneNumber));

        RuleFor(p => p.BookingPolicy)
            .NotNull().WithMessage("Online randevu kurallarını girin");
        RuleFor(p => p.BookingPolicy.MaxDaysAhead)
            .InclusiveBetween(1, 180).WithMessage("Online randevu 1 ile 180 gün ilerisi arasında olabilir")
            .When(p => p.BookingPolicy is not null);
        RuleFor(p => p.BookingPolicy.CancelHoursBefore)
            .InclusiveBetween(0, 72).WithMessage("İptal süresi 0 ile 72 saat arasında olabilir")
            .When(p => p.BookingPolicy is not null);
        RuleFor(p => p.BookingPolicy.MaxUpcomingPerPhone)
            .InclusiveBetween(1, 20).WithMessage("Telefon başına randevu sınırı 1 ile 20 arasında olabilir")
            .When(p => p.BookingPolicy is not null);
    }
}

internal sealed class SettingsUpdateCommandHandler(
    ITenantRepository tenantRepository,
    ITenantContext tenantContext,
    IUnitOfWork unitOfWork) : IRequestHandler<SettingsUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(SettingsUpdateCommand request, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.GetTenantId();
        var tenant = await tenantRepository.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
        if (tenant is null)
        {
            return Result<string>.Failure(404, "Salon bulunamadı");
        }

        Address? address = request.Address is null
            ? null
            : new Address(
                request.Address.City.Trim(),
                request.Address.District.Trim(),
                request.Address.FullAddress.Trim());

        tenant.SetName(new Name(request.Name.Trim()));
        tenant.SetAddress(address);
        tenant.SetPhoneNumber(string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : new PhoneNumber(request.PhoneNumber));
        tenant.SetBookingPolicy(request.BookingPolicy);

        tenantRepository.Update(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Ayarlar kaydedildi";
    }
}
