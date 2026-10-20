namespace Zamanla.Domain.Notifications.ValueObjects;

public sealed record NotificationType(string Value)
{
    public static NotificationType AppointmentCreated => new("Yeni randevu");
    public static NotificationType AppointmentCancelled => new("Randevu iptal edildi");
    public static NotificationType AppointmentRescheduled => new("Randevu saati değişti");
}
