namespace Zamanla.Domain.Appointments.ValueObjects;

public sealed record AppointmentHistory(string Title, string Description, DateTimeOffset CreatedAt);
