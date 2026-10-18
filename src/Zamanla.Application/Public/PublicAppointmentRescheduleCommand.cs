using FluentValidation;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Appointments;
using Zamanla.Application.Services;
using Zamanla.Application.Tenants;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Tenants;

namespace Zamanla.Application.Public;

// müşteri yetişemeyecekse randevu başlayana kadar linkten saatini değiştirebilir
public sealed record PublicAppointmentRescheduleCommand(
    string Token,
    Guid StaffId,
    DateTimeOffset StartAt) : IRequest<Result<string>>;

public sealed class PublicAppointmentRescheduleCommandValidator : AbstractValidator<PublicAppointmentRescheduleCommand>
{
    public PublicAppointmentRescheduleCommandValidator()
    {
        RuleFor(p => p.StaffId)
            .NotEmpty().WithMessage("Personel seçin");
        RuleFor(p => p.StartAt)
            .NotEmpty().WithMessage("Randevu saati seçin");
    }
}

internal sealed class PublicAppointmentRescheduleCommandHandler(
    IAppointmentRepository appointmentRepository,
    ITenantRepository tenantRepository,
    ITenantContext tenantContext,
    ISender sender,
    TimeProvider timeProvider) : IRequestHandler<PublicAppointmentRescheduleCommand, Result<string>>
{
    public async Task<Result<string>> Handle(PublicAppointmentRescheduleCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByManageTokenAsync(request.Token, cancellationToken);
        if (appointment is null)
        {
            return Result<string>.Failure(404, "Randevu bulunamadı");
        }

        if (appointment.IsFinished)
        {
            return Result<string>.Failure(400, "Bu randevu kapanmış, değiştirilemez");
        }

        var now = timeProvider.GetUtcNow();
        if (now >= appointment.Time.Start)
        {
            return Result<string>.Failure(400, "Randevu saati geldiği için linkten değiştirilemiyor, lütfen salonu arayın");
        }

        // panelde 15 dk tolerans var; müşteri geçmiş bir saate alamaz
        if (request.StartAt < now)
        {
            return Result<string>.Failure(400, "Geçmiş bir saate randevu verilemez");
        }

        var timeZone = await tenantRepository.GetTimeZoneAsync(tenantContext.GetTenantId(), cancellationToken);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, timeZone).DateTime);
        var day = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(request.StartAt, timeZone).DateTime);
        if (day > today.AddDays(OnlineBooking.MaxDaysAhead))
        {
            return Result<string>.Failure(400, $"Online randevu en fazla {OnlineBooking.MaxDaysAhead} gün sonrası için alınabilir");
        }

        return await sender.Send(
            new AppointmentRescheduleCommand(appointment.Id, request.StaffId, request.StartAt),
            cancellationToken);
    }
}
