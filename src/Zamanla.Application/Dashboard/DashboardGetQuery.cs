using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Services;
using Zamanla.Application.Tenants;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.Customers;
using Zamanla.Domain.SalonServices;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Tenants;

namespace Zamanla.Application.Dashboard;

// genel bakış sayfası: bugünün sayıları, bugün sıradaki randevular, son 7 günün grafiği
public sealed record DashboardGetQuery : IRequest<Result<DashboardDto>>;

public sealed class DashboardTodayDto
{
    // iptal edilenler hariç
    public int AppointmentCount { get; set; }
    // gelmeyenler hariç, takvimdeki "beklenen ciro" ile aynı
    public decimal ExpectedRevenue { get; set; }
    public int CompletedCount { get; set; }
    public int NoShowCount { get; set; }
    public int CancelledCount { get; set; }
}

public sealed class DashboardUpcomingDto
{
    public Guid Id { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public string CustomerFullName { get; set; } = default!;
    public string StaffFullName { get; set; } = default!;
    public List<string> ServiceNames { get; set; } = new();
    public bool IsOnline { get; set; }
}

public sealed class DashboardDayDto
{
    public DateOnly Date { get; set; }
    // iptal edilenler hariç
    public int AppointmentCount { get; set; }
    // sadece tamamlananlar; gerçekleşen ciro
    public decimal Revenue { get; set; }
}

public sealed class DashboardDto
{
    public DashboardTodayDto Today { get; set; } = new();
    public List<DashboardUpcomingDto> Upcoming { get; set; } = new();
    public List<DashboardDayDto> LastSevenDays { get; set; } = new();
}

internal sealed class DashboardGetQueryHandler(
    IAppointmentRepository appointmentRepository,
    ICustomerRepository customerRepository,
    IStaffRepository staffRepository,
    ISalonServiceRepository salonServiceRepository,
    ITenantRepository tenantRepository,
    ITenantContext tenantContext,
    TimeProvider timeProvider) : IRequestHandler<DashboardGetQuery, Result<DashboardDto>>
{
    private const int Days = 7;
    private const int UpcomingLimit = 5;

    public async Task<Result<DashboardDto>> Handle(DashboardGetQuery request, CancellationToken cancellationToken)
    {
        #region Günler
        // günler salonun saat dilimine göre; UTC gece yarısı Türkiye'de sabah 03:00
        var now = timeProvider.GetUtcNow();
        var timeZone = await tenantRepository.GetTimeZoneAsync(tenantContext.GetTenantId(), cancellationToken);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, timeZone).DateTime);
        var firstDay = today.AddDays(-(Days - 1));
        var rangeStart = StartOfDay(firstDay, timeZone);
        var rangeEnd = StartOfDay(today.AddDays(1), timeZone);
        #endregion

        var appointments = await appointmentRepository
            .Where(a => a.Time.Start >= rangeStart && a.Time.Start < rangeEnd)
            .Select(a => new
            {
                a.Id,
                a.Time.Start,
                a.Time.End,
                Status = a.Status.Value,
                Total = a.Total.Value,
                a.CustomerId,
                a.StaffId,
                Source = a.Source.Value,
                SalonServiceIds = a.SalonServices.Select(s => s.SalonServiceId).ToList()
            })
            .ToListAsync(cancellationToken);

        var cancelled = Status.Cancelled.Value;
        var completed = Status.Completed.Value;
        var noShow = Status.NoShow.Value;
        var confirmed = Status.Confirmed.Value;

        #region Bugün
        var todayAppointments = appointments
            .Where(a => LocalDate(a.Start, timeZone) == today)
            .ToList();
        var active = todayAppointments.Where(a => a.Status != cancelled).ToList();

        DashboardTodayDto todayDto = new()
        {
            AppointmentCount = active.Count,
            ExpectedRevenue = active.Where(a => a.Status != noShow).Sum(a => a.Total),
            CompletedCount = active.Count(a => a.Status == completed),
            NoShowCount = active.Count(a => a.Status == noShow),
            CancelledCount = todayAppointments.Count - active.Count
        };
        #endregion

        #region Sıradaki Randevular
        var upcoming = todayAppointments
            .Where(a => a.Status == confirmed && a.Start >= now)
            .OrderBy(a => a.Start)
            .Take(UpcomingLimit)
            .ToList();

        var customerIds = upcoming.Select(a => a.CustomerId).Distinct().ToList();
        var customerNames = await customerRepository
            .Where(c => customerIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.FirstName.Value + " " + c.LastName.Value, cancellationToken);
        var staffIds = upcoming.Select(a => a.StaffId).Distinct().ToList();
        var staffNames = await staffRepository.GetAllIncludingDeleted()
            .Where(s => staffIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.FirstName.Value + " " + s.LastName.Value, cancellationToken);
        var serviceIds = upcoming.SelectMany(a => a.SalonServiceIds).Distinct().ToList();
        var serviceNames = await salonServiceRepository.GetAllIncludingDeleted()
            .Where(s => serviceIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name.Value, cancellationToken);

        var online = Source.Online.Value;
        var upcomingDtos = upcoming.Select(a => new DashboardUpcomingDto
        {
            Id = a.Id,
            StartAt = a.Start,
            EndAt = a.End,
            CustomerFullName = customerNames[a.CustomerId],
            StaffFullName = staffNames[a.StaffId],
            ServiceNames = a.SalonServiceIds.Select(id => serviceNames[id]).ToList(),
            IsOnline = a.Source == online
        }).ToList();
        #endregion

        #region Son 7 Gün
        // randevusu olmayan gün de grafikte sıfır olarak görünsün
        var days = Enumerable.Range(0, Days)
            .Select(offset => firstDay.AddDays(offset))
            .Select(date =>
            {
                var dayAppointments = appointments
                    .Where(a => a.Status != cancelled && LocalDate(a.Start, timeZone) == date)
                    .ToList();
                return new DashboardDayDto
                {
                    Date = date,
                    AppointmentCount = dayAppointments.Count,
                    Revenue = dayAppointments.Where(a => a.Status == completed).Sum(a => a.Total)
                };
            })
            .ToList();
        #endregion

        return new DashboardDto
        {
            Today = todayDto,
            Upcoming = upcomingDtos,
            LastSevenDays = days
        };
    }

    private static DateTimeOffset StartOfDay(DateOnly date, TimeZoneInfo timeZone)
    {
        var localMidnight = date.ToDateTime(TimeOnly.MinValue);
        return new DateTimeOffset(localMidnight, timeZone.GetUtcOffset(localMidnight));
    }

    private static DateOnly LocalDate(DateTimeOffset value, TimeZoneInfo timeZone) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(value, timeZone).DateTime);
}
