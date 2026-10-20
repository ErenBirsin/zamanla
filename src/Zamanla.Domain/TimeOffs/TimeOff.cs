using Zamanla.Domain.Abstractions;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.Shared;

namespace Zamanla.Domain.TimeOffs;

// personel boşsa bütün salon kapalıdır: resmi tatil, bakım günü
public sealed class TimeOff : Entity, ITenantEntity
{
    private TimeOff() { }

    public TimeOff(Guid? staffId, DateTimeRange time, Note? note)
    {
        SetStaffId(staffId);
        SetTime(time);
        SetNote(note);
    }

    // kayıt sırasında TenantInterceptor doldurur
    public Guid TenantId { get; private set; }
    public Guid? StaffId { get; private set; }
    public DateTimeRange Time { get; private set; } = default!;
    public Note? Note { get; private set; }

    public bool IsSalonClosed => StaffId is null;

    #region Behaviors
    public void SetStaffId(Guid? staffId)
    {
        StaffId = staffId;
    }

    public void SetTime(DateTimeRange time)
    {
        Time = time;
    }

    public void SetNote(Note? note)
    {
        Note = note;
    }
    #endregion
}
