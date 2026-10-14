using Zamanla.Domain.Staffs.ValueObjects;

namespace Zamanla.Application.Staffs;

internal static class StaffRules
{
    // istemcideki --color-staff-* renkleriyle aynı
    public static readonly string[] Colors = ["peach", "sage", "lavender", "sky"];

    public static bool HaveNoOverlap(List<WorkingHour>? workingHours)
    {
        if (workingHours is null)
        {
            return true;
        }

        return workingHours
            .GroupBy(h => h.Day)
            .All(day =>
            {
                var ordered = day.OrderBy(h => h.StartTime).ToList();
                return ordered.Zip(ordered.Skip(1)).All(pair => pair.First.EndTime <= pair.Second.StartTime);
            });
    }
}
