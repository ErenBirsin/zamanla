using Zamanla.Domain.Abstractions;
using Zamanla.Domain.Notifications.ValueObjects;

namespace Zamanla.Domain.Notifications;

// müşterinin linkten yaptığı işlemler salon paneline bildirim olarak düşer;
// müşteri adı ve saat randevudan okunur, silinen müşterinin adı burada kalmaz
public sealed class Notification : Entity, ITenantEntity
{
    private Notification() { }

    public Notification(NotificationType type, Guid appointmentId)
    {
        SetType(type);
        SetAppointmentId(appointmentId);
    }

    // kayıt sırasında TenantInterceptor doldurur
    public Guid TenantId { get; private set; }
    public NotificationType Type { get; private set; } = default!;
    public Guid AppointmentId { get; private set; }
    public bool IsRead { get; private set; }

    #region Behaviors
    public void SetType(NotificationType type)
    {
        Type = type;
    }

    public void SetAppointmentId(Guid appointmentId)
    {
        AppointmentId = appointmentId;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
    #endregion
}
