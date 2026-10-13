using Zamanla.Domain.Abstractions;
using Zamanla.Domain.SalonServices;

namespace Zamanla.Application.SalonServices;

public sealed class SalonServiceDto : EntityDto
{
    public string Name { get; set; } = default!;
    public string Category { get; set; } = default!;
    public int DurationMinutes { get; set; }
    public int BufferMinutes { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}

public static class SalonServiceExtensions
{
    public static IQueryable<SalonServiceDto> MapTo(this IQueryable<EntityWithAuditDto<SalonService>> entities)
    {
        return entities.Select(s => new SalonServiceDto
        {
            Id = s.Entity.Id,
            Name = s.Entity.Name.Value,
            Category = s.Entity.Category.Value,
            DurationMinutes = s.Entity.Duration.Value,
            BufferMinutes = s.Entity.Buffer.Value,
            Price = s.Entity.Price.Value,
            IsActive = s.Entity.IsActive,
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
