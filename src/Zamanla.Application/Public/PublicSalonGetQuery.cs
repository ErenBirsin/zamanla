using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Services;
using Zamanla.Application.Staffs;
using Zamanla.Domain.SalonServices;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Tenants;

namespace Zamanla.Application.Public;

// randevu sayfasının ilk ekranı: salon adı, randevuya açık hizmetler ve personel
public sealed record PublicSalonGetQuery() : IRequest<Result<PublicSalonDto>>;

public sealed class PublicSalonDto
{
    public string Name { get; set; } = default!;
    public int BookingDaysAhead { get; set; }
    public List<PublicSalonServiceDto> Services { get; set; } = new();
    public List<PublicStaffDto> Staff { get; set; } = new();
}

public sealed class PublicSalonServiceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Category { get; set; } = default!;
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
}

public sealed class PublicStaffDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Color { get; set; } = default!;
    public List<Guid> SalonServiceIds { get; set; } = new();
    public List<StaffWorkingHourDto> WorkingHours { get; set; } = new();
}

internal sealed class PublicSalonGetQueryHandler(
    ITenantRepository tenantRepository,
    ISalonServiceRepository salonServiceRepository,
    IStaffRepository staffRepository,
    ITenantContext tenantContext) : IRequestHandler<PublicSalonGetQuery, Result<PublicSalonDto>>
{
    public async Task<Result<PublicSalonDto>> Handle(PublicSalonGetQuery request, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.GetTenantId();
        var name = await tenantRepository
            .Where(t => t.Id == tenantId)
            .Select(t => t.Name.Value)
            .FirstAsync(cancellationToken);

        var staffs = await staffRepository
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);
        var salonServices = await salonServiceRepository
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);

        // en az bir aktif personelin verdiği hizmet randevuya açıktır
        var bookableServices = salonServices
            .Where(service => staffs.Any(staff => staff.SalonServices.Any(s => s.SalonServiceId == service.Id)))
            .OrderBy(s => s.Category.Value)
            .ThenBy(s => s.Name.Value)
            .ToList();
        var bookableIds = bookableServices.Select(s => s.Id).ToHashSet();

        return new PublicSalonDto
        {
            Name = name,
            BookingDaysAhead = OnlineBooking.MaxDaysAhead,
            Services = bookableServices
                .Select(s => new PublicSalonServiceDto
                {
                    Id = s.Id,
                    Name = s.Name.Value,
                    Category = s.Category.Value,
                    DurationMinutes = s.Duration.Value,
                    Price = s.Price.Value
                })
                .ToList(),
            Staff = staffs
                .Where(staff => staff.SalonServices.Any(s => bookableIds.Contains(s.SalonServiceId)))
                .OrderBy(staff => staff.FirstName.Value)
                .Select(staff => new PublicStaffDto
                {
                    Id = staff.Id,
                    FirstName = staff.FirstName.Value,
                    LastName = staff.LastName.Value,
                    Color = staff.Color.Value,
                    SalonServiceIds = staff.SalonServices
                        .Select(s => s.SalonServiceId)
                        .Where(bookableIds.Contains)
                        .ToList(),
                    WorkingHours = staff.WorkingHours
                        .OrderBy(h => h.Day)
                        .ThenBy(h => h.StartTime)
                        .Select(h => new StaffWorkingHourDto
                        {
                            Day = (int)h.Day,
                            StartTime = h.StartTime,
                            EndTime = h.EndTime
                        })
                        .ToList()
                })
                .ToList()
        };
    }
}
