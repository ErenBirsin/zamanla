namespace Zamanla.Domain.Staffs.ValueObjects;

public sealed record WorkingHour(DayOfWeek Day, TimeOnly StartTime, TimeOnly EndTime);
