using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.Staffs.ValueObjects;

namespace Zamanla.Application.Appointments;

// veritabanına dokunmaz: girdiler verilir, o gün için boş başlangıç saatleri döner
public static class AvailabilityCalculator
{
    public const int SlotStepMinutes = 15;

    // busyRanges: mevcut randevular, başlangıç → BlockedUntil (hazırlık payı dahil)
    public static List<DateTimeOffset> GetSlots(
        DateOnly date,
        TimeZoneInfo timeZone,
        IEnumerable<WorkingHour> workingHours,
        IEnumerable<DateTimeRange> busyRanges,
        int duration,
        int buffer,
        DateTimeOffset now)
    {
        var busy = busyRanges.ToList();
        var slots = new List<DateTimeOffset>();

        var dayHours = workingHours
            .Where(h => h.Day == date.DayOfWeek)
            .OrderBy(h => h.StartTime);

        foreach (var workingHour in dayHours)
        {
            var start = ToDateTimeOffset(date, workingHour.StartTime, timeZone);
            var end = ToDateTimeOffset(date, workingHour.EndTime, timeZone);

            // hizmet çalışma aralığına sığmalı; hazırlık payı kapanıştan sonraya taşabilir
            for (var slot = start; slot.AddMinutes(duration) <= end; slot = slot.AddMinutes(SlotStepMinutes))
            {
                var blockedUntil = slot.AddMinutes(duration + buffer);
                var isFree = !busy.Any(b => b.Start < blockedUntil && b.End > slot);

                if (slot >= now && isFree)
                {
                    slots.Add(slot);
                }
            }
        }

        return slots;
    }

    private static DateTimeOffset ToDateTimeOffset(DateOnly date, TimeOnly time, TimeZoneInfo timeZone)
    {
        var local = date.ToDateTime(time);
        return new DateTimeOffset(local, timeZone.GetUtcOffset(local));
    }
}
