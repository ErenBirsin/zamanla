using Microsoft.EntityFrameworkCore;
using TS.Result;
using Zamanla.Application.Appointments;
using Zamanla.Application.Tenants;
using Zamanla.Application.TimeOffs;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.SalonServices;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Tenants;
using Zamanla.Domain.TimeOffs;

namespace Zamanla.Application.Services;

// panelden ve online sayfadan randevu oluşturmanın ortak kontrolleri; kaydetmeyi çağıran yapar
public sealed class AppointmentBookingService(
    IAppointmentRepository appointmentRepository,
    IStaffRepository staffRepository,
    ISalonServiceRepository salonServiceRepository,
    ITenantRepository tenantRepository,
    ITimeOffRepository timeOffRepository,
    ITenantContext tenantContext)
{
    public async Task<Result<Appointment>> CreateAsync(
        Guid customerId,
        Guid staffId,
        List<Guid> salonServiceIds,
        DateTimeOffset startAt,
        Source source,
        Note? note,
        DateTimeOffset now,
        TimeSpan pastTolerance,
        CancellationToken cancellationToken)
    {
        #region Personel ve Hizmet Kontrolü
        var staff = await staffRepository.FirstOrDefaultAsync(s => s.Id == staffId, cancellationToken);
        if (staff is null)
        {
            return Result<Appointment>.Failure(404, "Personel bulunamadı");
        }

        if (!staff.IsActive)
        {
            return Result<Appointment>.Failure(400, "Pasif personele randevu verilemez");
        }

        var ids = salonServiceIds.Distinct().ToList();
        var salonServices = await salonServiceRepository
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);
        if (salonServices.Count != ids.Count)
        {
            return Result<Appointment>.Failure(404, "Seçilen hizmetlerden biri bulunamadı");
        }

        if (salonServices.Any(s => !s.IsActive))
        {
            return Result<Appointment>.Failure(400, "Pasif bir hizmete randevu verilemez");
        }

        var staffGivesAll = ids.All(id => staff.SalonServices.Any(s => s.SalonServiceId == id));
        if (!staffGivesAll)
        {
            return Result<Appointment>.Failure(400, "Personel seçilen hizmetlerden birini vermiyor");
        }
        #endregion

        #region Randevu Objesinin Oluşturulması
        // hizmetler seçilme sırasıyla arka arkaya yapılır
        var appointmentSalonServices = ids
            .Select(id => salonServices.First(s => s.Id == id))
            .Select(s => new AppointmentSalonService(s.Id, s.Duration.Value, s.Buffer.Value, s.Price.Value));

        Appointment appointment = new(
            customerId,
            staff.Id,
            appointmentSalonServices,
            startAt,
            source,
            note,
            now);
        #endregion

        #region Saat ve Çakışma Kontrolü
        if (appointment.Time.Start < now - pastTolerance)
        {
            return Result<Appointment>.Failure(400, "Geçmiş bir saate randevu verilemez");
        }

        var timeZone = await tenantRepository.GetTimeZoneAsync(tenantContext.GetTenantId(), cancellationToken);
        if (!AvailabilityCalculator.IsWithinWorkingHours(appointment.Time, timeZone, staff.WorkingHours))
        {
            return Result<Appointment>.Failure(400, "Seçilen saat personelin çalışma saatleri dışında");
        }

        var timeOffConflict = await timeOffRepository.GetTimeOffConflictAsync(staff.Id, appointment.Time, cancellationToken);
        if (timeOffConflict is not null)
        {
            return Result<Appointment>.Failure(400, timeOffConflict);
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
            return Result<Appointment>.Failure(409, "Bu saatte personelin başka bir randevusu var");
        }
        #endregion

        appointmentRepository.Add(appointment);
        // aynı anda gelen ikinci kayıt BookingVersion yüzünden kaydedilemez
        staff.IncreaseBookingVersion();
        staffRepository.Update(staff);

        return appointment;
    }
}
