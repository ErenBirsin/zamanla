using Zamanla.Domain.Abstractions;
using Zamanla.Domain.Staffs;

namespace Zamanla.Application.Staffs;

public sealed class StaffDto : EntityDto
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Color { get; set; } = default!;
    public bool IsActive { get; set; }
    public List<StaffWorkingHourDto> WorkingHours { get; set; } = new();
    public List<Guid> SalonServiceIds { get; set; } = new();
}

public sealed class StaffWorkingHourDto
{
    // 0 = Pazar … 6 = Cumartesi 
    public int Day { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}

public static class StaffExtensions
{
    public static IQueryable<StaffDto> MapTo(this IQueryable<EntityWithAuditDto<Staff>> entities)
    {
        return entities.Select(s => new StaffDto
        {
            Id = s.Entity.Id,
            FirstName = s.Entity.FirstName.Value,
            LastName = s.Entity.LastName.Value,
            Color = s.Entity.Color.Value,
            IsActive = s.Entity.IsActive,
            WorkingHours = s.Entity.WorkingHours
                .OrderBy(h => h.Day)
                .ThenBy(h => h.StartTime)
                .Select(h => new StaffWorkingHourDto
                {
                    Day = (int)h.Day,
                    StartTime = h.StartTime,
                    EndTime = h.EndTime
                })
                .ToList(),
            SalonServiceIds = s.Entity.SalonServices.Select(x => x.SalonServiceId).ToList(),
            CreatedAt = s.Entity.CreatedAt,
            CreatedBy = s.Entity.CreatedBy,
            CreatedFullName = s.CreatedUser != null
                ? s.CreatedUser.FirstName.Value + " " + s.CreatedUser.LastName.Value
                : null,
            UpdatedAt = s.Entity.UpdatedAt,
            UpdatedBy = s.Entity.UpdatedBy,
            UpdatedFullName = s.UpdatedUser != null
                ? s.UpdatedUser.FirstName.Value + " " + s.UpdatedUser.LastName.Value
                : null,
        });
    }
}
