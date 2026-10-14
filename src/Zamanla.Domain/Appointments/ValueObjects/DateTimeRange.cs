namespace Zamanla.Domain.Appointments.ValueObjects;

// UTC saklanır, işletmenin saat diliminde gösterilir
public sealed record DateTimeRange(DateTimeOffset Start, DateTimeOffset End);
