using GenericRepository;
using TS.MediatR;
using TS.Result;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Customers;

namespace Zamanla.Application.Appointments;

public sealed record AppointmentNoShowCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class AppointmentNoShowCommandHandler(
    IAppointmentRepository appointmentRepository,
    ICustomerRepository customerRepository,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork) : IRequestHandler<AppointmentNoShowCommand, Result<string>>
{
    public async Task<Result<string>> Handle(AppointmentNoShowCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (appointment is null)
        {
            return Result<string>.Failure(404, "Randevu bulunamadı");
        }

        if (appointment.IsFinished)
        {
            return Result<string>.Failure(400, "Bu randevu kapanmış, değiştirilemez");
        }

        var now = timeProvider.GetUtcNow();
        if (appointment.Time.Start > now)
        {
            return Result<string>.Failure(400, "Başlamamış bir randevu için gelmedi işaretlenemez");
        }

        var customer = await customerRepository.FirstAsync(c => c.Id == appointment.CustomerId, cancellationToken);

        appointment.MarkAsNoShow(now);
        appointmentRepository.Update(appointment);
        customer.IncreaseNoShowCount();
        customerRepository.Update(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Randevu gelmedi olarak işaretlendi";
    }
}
