using GenericRepository;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Permissions;
using Zamanla.Domain.Appointments;

namespace Zamanla.Application.Appointments;

public sealed record AppointmentCompleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class AppointmentCompleteCommandHandler(
    IAppointmentRepository appointmentRepository,
    TimeProvider timeProvider,
    AccessService accessService,
    IUnitOfWork unitOfWork) : IRequestHandler<AppointmentCompleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(AppointmentCompleteCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (appointment is null)
        {
            return Result<string>.Failure(404, "Randevu bulunamadı");
        }

        await accessService.EnsureCanManageAppointmentAsync(appointment.StaffId, cancellationToken);

        if (appointment.IsFinished)
        {
            return Result<string>.Failure(400, "Bu randevu kapanmış, değiştirilemez");
        }

        var now = timeProvider.GetUtcNow();
        if (appointment.Time.Start > now)
        {
            return Result<string>.Failure(400, "Başlamamış bir randevu tamamlandı olarak işaretlenemez");
        }

        appointment.Complete(now);
        appointmentRepository.Update(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Randevu tamamlandı";
    }
}
