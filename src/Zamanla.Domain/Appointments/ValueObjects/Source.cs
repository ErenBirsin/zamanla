namespace Zamanla.Domain.Appointments.ValueObjects;
public sealed record Source(string Value)
{
    public static Source Panel => new("Panel");
    public static Source Online => new("Online");
}
