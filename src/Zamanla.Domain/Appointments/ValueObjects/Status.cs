namespace Zamanla.Domain.Appointments.ValueObjects;

public sealed record Status(string Value)
{
    public static Status Confirmed => new("Onaylandı");
    public static Status Completed => new("Tamamlandı");
    public static Status NoShow => new("Gelmedi");
    public static Status Cancelled => new("İptal Edildi");
}
