namespace Zamanla.Domain.Tenants.ValueObjects;

// online randevu kuralları; salon sahibi ayarlardan değiştirir
public sealed record BookingPolicy(
    int MaxDaysAhead,
    int CancelHoursBefore,
    int MaxUpcomingPerPhone)
{
    public static BookingPolicy Default => new(60, 3, 5);
}
