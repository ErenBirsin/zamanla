using TS.MediatR;
using TS.Result;
using Zamanla.Application.Appointments;
using Zamanla.Application.Services;
using Zamanla.Application.Tenants;
using Zamanla.Domain.Tenants;

namespace Zamanla.Application.Public;

// hesap paneldekiyle aynı; müşteri taşıma modunu kullanamaz ve en fazla belirli gün sonrasını görür
public sealed record PublicAvailabilityGetQuery(
    DateOnly Date,
    List<Guid> SalonServiceIds,
    Guid? StaffId) : IRequest<Result<List<AvailabilitySlotDto>>>;

internal sealed class PublicAvailabilityGetQueryHandler(
    ISender sender,
    ITenantRepository tenantRepository,
    ITenantContext tenantContext,
    TimeProvider timeProvider) : IRequestHandler<PublicAvailabilityGetQuery, Result<List<AvailabilitySlotDto>>>
{
    public async Task<Result<List<AvailabilitySlotDto>>> Handle(PublicAvailabilityGetQuery request, CancellationToken cancellationToken)
    {
        var timeZone = await tenantRepository.GetTimeZoneAsync(tenantContext.GetTenantId(), cancellationToken);
        var localNow = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), timeZone);
        var today = DateOnly.FromDateTime(localNow.DateTime);
        var policy = await tenantRepository.GetBookingPolicyAsync(tenantContext.GetTenantId(), cancellationToken);
        if (request.Date > today.AddDays(policy.MaxDaysAhead))
        {
            return Result<List<AvailabilitySlotDto>>.Failure(400, $"Online randevu en fazla {policy.MaxDaysAhead} gün sonrası için alınabilir");
        }

        return await sender.Send(
            new AvailabilityGetQuery(request.Date, request.SalonServiceIds, request.StaffId, null),
            cancellationToken);
    }
}
