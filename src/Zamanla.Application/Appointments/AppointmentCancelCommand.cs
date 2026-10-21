using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Permissions;
using Zamanla.Domain.Appointments;

namespace Zamanla.Application.Appointments;

public sealed record AppointmentCancelCommand(Guid Id, string? Reason) : IRequest<Result<string>>;

public sealed class AppointmentCancelCommandValidator : AbstractValidator<AppointmentCancelCommand>
{
    public AppointmentCancelCommandValidator()
    {
        RuleFor(p => p.Reason)
            .MaximumLength(500).WithMessage("İptal nedeni en fazla 500 karakter olabilir");
    }
}

internal sealed class AppointmentCancelCommandHandler(
    IAppointmentRepository appointmentRepository,
    TimeProvider timeProvider,
    AccessService accessService,
    IUnitOfWork unitOfWork) : IRequestHandler<AppointmentCancelCommand, Result<string>>
{
    public async Task<Result<string>> Handle(AppointmentCancelCommand request, CancellationToken cancellationToken)
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

        var description = string.IsNullOrWhiteSpace(request.Reason)
            ? "Salon tarafından iptal edildi"
            : request.Reason.Trim();

        appointment.Cancel(description, timeProvider.GetUtcNow());
        appointmentRepository.Update(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Randevu iptal edildi";
    }
}
