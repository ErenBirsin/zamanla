using Microsoft.EntityFrameworkCore;
using Zamanla.Domain.Tenants;
using Zamanla.Domain.Tenants.ValueObjects;

namespace Zamanla.Application.Tenants;

public static class TenantRepositoryExtensions
{
    // randevular UTC saklanır, çalışma saatleri işletmenin saat diliminde karşılaştırılır
    public static async Task<TimeZoneInfo> GetTimeZoneAsync(
        this ITenantRepository tenantRepository,
        Guid? tenantId,
        CancellationToken cancellationToken = default)
    {
        var timeZoneId = await tenantRepository
            .Where(t => t.Id == tenantId)
            .Select(t => t.TimeZoneId.Value)
            .FirstAsync(cancellationToken);

        return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }


    // online randevu kuralları salonun ayarlarından gelir
    public static async Task<BookingPolicy> GetBookingPolicyAsync(
        this ITenantRepository tenantRepository,
        Guid? tenantId,
        CancellationToken cancellationToken = default)
    {
        return await tenantRepository
            .Where(t => t.Id == tenantId)
            .Select(t => new BookingPolicy(
                t.BookingPolicy.MaxDaysAhead,
                t.BookingPolicy.CancelHoursBefore,
                t.BookingPolicy.MaxUpcomingPerPhone))
            .FirstAsync(cancellationToken);
    }
}
