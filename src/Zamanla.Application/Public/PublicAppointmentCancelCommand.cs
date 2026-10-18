using TS.MediatR;
using TS.Result;
using Zamanla.Application.Appointments;
using Zamanla.Domain.Appointments;

namespace Zamanla.Application.Public;

// müşteri linkten iptal eder; son saatlerde iptal salonu zor durumda bırakır
public sealed record PublicAppointmentCancelCommand(string Token) : IRequest<Result<string>>;

internal sealed class PublicAppointmentCancelCommandHandler(
    IAppointmentRepository appointmentRepository,
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

        var cancelDeadline = appointment.Time.Start.AddHours(-OnlineBooking.CancelHoursBefore);
        if (timeProvider.GetUtcNow() >= cancelDeadline)
        {
            return Result<string>.Failure(400, $"Randevuya {OnlineBooking.CancelHoursBefore} saatten az kaldığı için linkten iptal edilemiyor, lütfen salonu arayın");
        }

        return await sender.Send(new AppointmentCancelCommand(appointment.Id, "Müşteri linkten iptal etti"), cancellationToken);
    }
}
