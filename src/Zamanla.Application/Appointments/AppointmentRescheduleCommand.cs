using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Services;
using Zamanla.Application.Tenants;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Tenants;

namespace Zamanla.Application.Appointments;

public sealed record AppointmentRescheduleCommand(
    Guid Id,
    Guid StaffId,
    DateTimeOffset StartAt) : IRequest<Result<string>>;

public sealed class AppointmentRescheduleCommandValidator : AbstractValidator<AppointmentRescheduleCommand>
{
    public AppointmentRescheduleCommandValidator()
    {
        RuleFor(p => p.StaffId)
            .NotEmpty().WithMessage("Personel seçin");
        RuleFor(p => p.StartAt)
            .NotEmpty().WithMessage("Randevu saati seçin");
    }
}

internal sealed class AppointmentRescheduleCommandHandler(
    IAppointmentRepository appointmentRepository,
    IStaffRepository staffRepository,
    ITenantRepository tenantRepository,
    ITenantContext tenantContext,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork) : IRequestHandler<AppointmentRescheduleCommand, Result<string>>
{
    // oluşturmadaki toleransla aynı
    private static readonly TimeSpan PastTolerance = TimeSpan.FromMinutes(15);

    public async Task<Result<string>> Handle(AppointmentRescheduleCommand request, CancellationToken cancellationToken)
    {
        #region Randevu ve Personel Kontrolü
        var appointment = await appointmentRepository.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (appointment is null)
        {
            return Result<string>.Failure(404, "Randevu bulunamadı");
        }

        if (appointment.IsFinished)
        {
            return Result<string>.Failure(400, "Bu randevu kapanmış, değiştirilemez");
        }

        var staff = await staffRepository.FirstOrDefaultAsync(s => s.Id == request.StaffId, cancellationToken);
        if (staff is null)
        {
            return Result<string>.Failure(404, "Personel bulunamadı");
        }

        if (!staff.IsActive)
        {
            return Result<string>.Failure(400, "Pasif personele randevu verilemez");
        }

        var staffGivesAll = appointment.SalonServices
            .All(service => staff.SalonServices.Any(s => s.SalonServiceId == service.SalonServiceId));
        if (!staffGivesAll)
        {
            return Result<string>.Failure(400, "Personel bu randevudaki hizmetlerden birini vermiyor");
        }
        #endregion

        var now = timeProvider.GetUtcNow();
        // süre ve fiyat değişmez; yalnızca personel ve saat
        appointment.Reschedule(staff.Id, request.StartAt, now);

        #region Saat ve Çakışma Kontrolü
        if (appointment.Time.Start < now - PastTolerance)
        {
            return Result<string>.Failure(400, "Geçmiş bir saate randevu verilemez");
        }

        var timeZone = await tenantRepository.GetTimeZoneAsync(tenantContext.GetTenantId(), cancellationToken);

        if (!AvailabilityCalculator.IsWithinWorkingHours(appointment.Time, timeZone, staff.WorkingHours))
        {
            return Result<string>.Failure(400, "Seçilen saat personelin çalışma saatleri dışında");
        }

        var appointmentId = appointment.Id;
        var start = appointment.Time.Start;
        var blockedUntil = appointment.BlockedUntil;
        var confirmed = Status.Confirmed.Value;
        // randevunun kendi eski saati çakışma sayılmaz
        var overlaps = await appointmentRepository.AnyAsync(a =>
            a.Id != appointmentId
            && a.StaffId == staff.Id
            && a.Status.Value == confirmed
            && a.Time.Start < blockedUntil
            && a.BlockedUntil > start, cancellationToken);
        if (overlaps)
        {
            return Result<string>.Failure(409, "Bu saatte personelin başka bir randevusu var");
        }
        #endregion

        appointmentRepository.Update(appointment);
        staff.IncreaseBookingVersion();
        staffRepository.Update(staff);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<string>.Failure(409, "Aynı anda başka bir randevu kaydedildi, lütfen tekrar deneyin");
        }

        return "Randevu taşındı";
    }
}
