using TS.MediatR;
using TS.Result;
using Zamanla.Application.Appointments;
using Zamanla.Application.Services;
using Zamanla.Application.Tenants;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Tenants;

namespace Zamanla.Application.Public;

// saat değiştirirken boş saatler; randevunun kendi saati boş sayılır
public sealed record PublicRescheduleAvailabilityGetQuery(
    string Token,
    DateOnly Date,
    Guid? StaffId) : IRequest<Result<List<AvailabilitySlotDto>>>;

internal sealed class PublicRescheduleAvailabilityGetQueryHandler(
    IAppointmentRepository appointmentRepository,
    ITenantRepository tenantRepository,
    ITenantContext tenantContext,
    ISender sender,
    TimeProvider timeProvider) : IRequestHandler<PublicRescheduleAvailabilityGetQuery, Result<List<AvailabilitySlotDto>>>
{
    public async Task<Result<List<AvailabilitySlotDto>>> Handle(PublicRescheduleAvailabilityGetQuery request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByManageTokenAsync(request.Token, cancellationToken);
        if (appointment is null)
        {
            return Result<List<AvailabilitySlotDto>>.Failure(404, "Randevu bulunamadı");
        }

        var timeZone = await tenantRepository.GetTimeZoneAsync(tenantContext.GetTenantId(), cancellationToken);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), timeZone).DateTime);
        var policy = await tenantRepository.GetBookingPolicyAsync(tenantContext.GetTenantId(), cancellationToken);
        if (request.Date > today.AddDays(policy.MaxDaysAhead))
        {
            return Result<List<AvailabilitySlotDto>>.Failure(400, $"Online randevu en fazla {policy.MaxDaysAhead} gün sonrası için alınabilir");
        }

        return await sender.Send(
            new AvailabilityGetQuery(request.Date, [], request.StaffId, appointment.Id),
            cancellationToken);
    }
}
