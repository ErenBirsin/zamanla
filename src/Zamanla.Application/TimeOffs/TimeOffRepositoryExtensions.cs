using Microsoft.EntityFrameworkCore;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.TimeOffs;

namespace Zamanla.Application.TimeOffs;

public static class TimeOffRepositoryExtensions
{
    // randevu personelin iznine ya da salonun kapalı gününe denk geliyorsa nedeni döner
    public static async Task<string?> GetTimeOffConflictAsync(
        this ITimeOffRepository timeOffRepository,
        Guid staffId,
        DateTimeRange time,
        CancellationToken cancellationToken = default)
    {
        var timeOff = await timeOffRepository
            .Where(t => (t.StaffId == null || t.StaffId == staffId)
                && t.Time.Start < time.End
                && t.Time.End > time.Start)
            .Select(t => new { t.StaffId })
            .FirstOrDefaultAsync(cancellationToken);

        if (timeOff is null)
        {
            return null;
        }

        return timeOff.StaffId is null ? "Salon bu tarihte kapalı" : "Personel bu saatte izinli";
    }
}
