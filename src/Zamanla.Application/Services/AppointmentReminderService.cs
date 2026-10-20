using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Appointments.ValueObjects;

namespace Zamanla.Application.Services;

// o anki salonun 24 saat içinde başlayacak randevularına hatırlatma gönderir
public sealed class AppointmentReminderService(
    IAppointmentRepository appointmentRepository,
    IAppointmentNotificationService notificationService,
    IUnitOfWork unitOfWork)
{
    public const int HoursBefore = 24;

    public async Task<int> SendDueRemindersAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var until = now.AddHours(HoursBefore);
        var confirmed = Status.Confirmed.Value;
        var appointments = await appointmentRepository
            .WhereWithTracking(a => a.Status.Value == confirmed
                && a.ReminderSentAt == null
                && a.Time.Start > now
                && a.Time.Start <= until
                // 24 saatten az kala alınan randevuya onay e-postası yeter
                && a.CreatedAt < a.Time.Start.AddHours(-HoursBefore))
            .ToListAsync(cancellationToken);

        var sentCount = 0;
        foreach (var appointment in appointments)
        {
            if (!await notificationService.SendReminderAsync(appointment.Id, cancellationToken))
            {
                continue;
            }

            appointment.SetReminderSentAt(now);
            appointmentRepository.Update(appointment);
            sentCount++;
        }

        if (sentCount > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return sentCount;
    }
}
