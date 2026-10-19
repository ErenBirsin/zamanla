using TS.MediatR;
using TS.Result;
using Zamanla.Application.Appointments;
using Zamanla.Application.Services;
using Zamanla.Application.Tenants;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Tenants;

namespace Zamanla.Application.Public;

// müşteri linkten iptal eder; son saatlerde iptal salonu zor durumda bırakır
public sealed record PublicAppointmentCancelCommand(string Token) : IRequest<Result<string>>;

internal sealed class PublicAppointmentCancelCommandHandler(
    IAppointmentRepository appointmentRepository,
    ITenantRepository tenantRepository,
    ITenantContext tenantContext,
    ISender sender,
    TimeProvider timeProvider) : IRequestHandler<PublicAppointmentCancelCommand, Result<string>>
{
    public async Task<Result<string>> Handle(PublicAppointmentCancelCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByManageTokenAsync(request.Token, cancellationToken);
        if (appointment is null)
        {
            return Result<string>.Failure(404, "Randevu bulunamadı");
        }

        if (appointment.IsFinished)
        {
            return Result<string>.Failure(400, "Bu randevu kapanmış, değiştirilemez");
        }

        var policy = await tenantRepository.GetBookingPolicyAsync(tenantContext.GetTenantId(), cancellationToken);
        var cancelDeadline = appointment.Time.Start.AddHours(-policy.CancelHoursBefore);
        if (timeProvider.GetUtcNow() >= cancelDeadline)
        {
            return Result<string>.Failure(400, $"Randevuya {policy.CancelHoursBefore} saatten az kaldığı için linkten iptal edilemiyor, lütfen salonu arayın");
        }

        return await sender.Send(new AppointmentCancelCommand(appointment.Id, "Müşteri linkten iptal etti"), cancellationToken);
    }
}
