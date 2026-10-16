using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Services;
using Zamanla.Application.Tenants;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.Customers;
using Zamanla.Domain.SalonServices;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Tenants;

namespace Zamanla.Application.Appointments;

public sealed record AppointmentCreateCommand(
    Guid CustomerId,
    Guid StaffId,
    List<Guid> SalonServiceIds,
    DateTimeOffset StartAt,
    string? Note) : IRequest<Result<string>>;

public sealed class AppointmentCreateCommandValidator : AbstractValidator<AppointmentCreateCommand>
{
    public AppointmentCreateCommandValidator()
    {
        RuleFor(p => p.CustomerId)
            .NotEmpty().WithMessage("Müşteri seçin");
        RuleFor(p => p.StaffId)
            .NotEmpty().WithMessage("Personel seçin");
        RuleFor(p => p.SalonServiceIds)
            .NotEmpty().WithMessage("En az bir hizmet seçin");
        RuleFor(p => p.StartAt)
            .NotEmpty().WithMessage("Randevu saati seçin");
        RuleFor(p => p.Note)
            .MaximumLength(500).WithMessage("Not en fazla 500 karakter olabilir");
    }
}

internal sealed class AppointmentCreateCommandHandler(
    IAppointmentRepository appointmentRepository,
    ICustomerRepository customerRepository,
    IStaffRepository staffRepository,
    ISalonServiceRepository salonServiceRepository,
    ITenantRepository tenantRepository,
    ITenantContext tenantContext,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork) : IRequestHandler<AppointmentCreateCommand, Result<string>>
{
    // içeri girmiş müşterinin randevusu panelden birkaç dakika geriden girilebilsin
    private static readonly TimeSpan PastTolerance = TimeSpan.FromMinutes(15);

    public async Task<Result<string>> Handle(AppointmentCreateCommand request, CancellationToken cancellationToken)
    {
        #region Müşteri, Personel ve Hizmet Kontrolü
        var isCustomerExists = await customerRepository.AnyAsync(
            c => c.Id == request.CustomerId && !c.IsAnonymized, cancellationToken);
        if (!isCustomerExists)
        {
            return Result<string>.Failure(404, "Müşteri bulunamadı");
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

        var salonServiceIds = request.SalonServiceIds.Distinct().ToList();
        var salonServices = await salonServiceRepository
            .Where(s => salonServiceIds.Contains(s.Id))
            .ToListAsync(cancellationToken);
        if (salonServices.Count != salonServiceIds.Count)
        {
            return Result<string>.Failure(404, "Seçilen hizmetlerden biri bulunamadı");
        }

        if (salonServices.Any(s => !s.IsActive))
        {
            return Result<string>.Failure(400, "Pasif bir hizmete randevu verilemez");
        }

        var staffGivesAll = salonServiceIds.All(id => staff.SalonServices.Any(s => s.SalonServiceId == id));
        if (!staffGivesAll)
        {
            return Result<string>.Failure(400, "Personel seçilen hizmetlerden birini vermiyor");
        }
        #endregion

        #region Randevu Objesinin Oluşturulması
        var now = timeProvider.GetUtcNow();
        // hizmetler seçilme sırasıyla arka arkaya yapılır
        var appointmentSalonServices = salonServiceIds
            .Select(id => salonServices.First(s => s.Id == id))
            .Select(s => new AppointmentSalonService(s.Id, s.Duration.Value, s.Buffer.Value, s.Price.Value));
        Note? note = string.IsNullOrWhiteSpace(request.Note) ? null : new Note(request.Note.Trim());

        Appointment appointment = new(
            request.CustomerId,
            staff.Id,
            appointmentSalonServices,
            request.StartAt,
            Source.Panel,
            note,
            now);
        #endregion

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

        var start = appointment.Time.Start;
        var blockedUntil = appointment.BlockedUntil;
        var confirmed = Status.Confirmed.Value;
        var overlaps = await appointmentRepository.AnyAsync(a =>
            a.StaffId == staff.Id
            && a.Status.Value == confirmed
            && a.Time.Start < blockedUntil
            && a.BlockedUntil > start, cancellationToken);
        if (overlaps)
        {
            return Result<string>.Failure(409, "Bu saatte personelin başka bir randevusu var");
        }
        #endregion

        appointmentRepository.Add(appointment);
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

        return "Randevu oluşturuldu";
    }
}
