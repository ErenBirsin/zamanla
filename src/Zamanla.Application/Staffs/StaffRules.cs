using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.Customers;
using Zamanla.Domain.Staffs.ValueObjects;

namespace Zamanla.Application.Staffs;

internal sealed record StaffAppointment(
    DateTimeOffset Start,
    DateTimeOffset End,
    List<Guid> SalonServiceIds,
    string CustomerFullName);

internal static class StaffRules
{
    // istemcideki --color-staff-* renkleriyle aynı
    public static readonly string[] Colors = ["peach", "sage", "lavender", "sky"];

    public static bool HaveNoOverlap(List<WorkingHour>? workingHours)
    {
        if (workingHours is null)
        {
            return true;
        }

        return workingHours
            .GroupBy(h => h.Day)
            .All(day =>
            {
                var ordered = day.OrderBy(h => h.StartTime).ToList();
                return ordered.Zip(ordered.Skip(1)).All(pair => pair.First.EndTime <= pair.Second.StartTime);
            });
    }

    // personelin ileri tarihli, iptal edilmemiş randevuları
    public static async Task<List<StaffAppointment>> GetUpcomingAppointmentsAsync(
        IAppointmentRepository appointmentRepository,
        ICustomerRepository customerRepository,
        Guid staffId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var confirmed = Status.Confirmed.Value;

        var appointments = await appointmentRepository
            .Where(a => a.StaffId == staffId && a.Status.Value == confirmed && a.Time.Start >= now)
            .Join(customerRepository.GetAll(), a => a.CustomerId, c => c.Id, (a, c) => new StaffAppointment(
                a.Time.Start,
                a.Time.End,
                a.SalonServices.Select(s => s.SalonServiceId).ToList(),
                c.FirstName.Value + " " + c.LastName.Value))
            .ToListAsync(cancellationToken);

        return appointments.OrderBy(a => a.Start).ToList();
    }

    // salon sahibi ne yapacağını bilsin diye ilk üç randevu yazılır
    public static string AppointmentConflictMessage(
        string action,
        List<StaffAppointment> appointments,
        TimeZoneInfo timeZone)
    {
        var culture = CultureInfo.GetCultureInfo("tr-TR");
        var listed = appointments
            .Take(3)
            .Select(a => $"{TimeZoneInfo.ConvertTime(a.Start, timeZone).ToString("d MMMM HH:mm", culture)} {a.CustomerFullName}");

        var message = $"{action} {appointments.Count} randevuyu etkiliyor. "
            + $"Önce bu randevuları taşıyın ya da iptal edin: {string.Join(", ", listed)}";

        if (appointments.Count > 3)
        {
            message += $" ve {appointments.Count - 3} randevu daha";
        }

        return message;
    }
}
