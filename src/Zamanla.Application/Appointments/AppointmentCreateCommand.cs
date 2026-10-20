using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Services;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.Customers;
using Zamanla.Domain.Shared;

namespace Zamanla.Application.Appointments;

public sealed record AppointmentCreateCommand(
    Guid CustomerId,
    Guid StaffId,
    List<Guid> SalonServiceIds,
    DateTimeOffset StartAt,
    string? Note) : IRequest<Result<string>>;

public sealed class AppointmentCreateCommandValidator : AbstractValidator<AppointmentCreateCommand>
{
    public AppointmentCreateCommandValidator()
    {
        RuleFor(p => p.CustomerId)
            .NotEmpty().WithMessage("Müşteri seçin");
        RuleFor(p => p.StaffId)
            .NotEmpty().WithMessage("Personel seçin");
        RuleFor(p => p.SalonServiceIds)
            .NotEmpty().WithMessage("En az bir hizmet seçin");
        RuleFor(p => p.StartAt)
            .NotEmpty().WithMessage("Randevu saati seçin");
        RuleFor(p => p.Note)
            .MaximumLength(500).WithMessage("Not en fazla 500 karakter olabilir");
    }
}

internal sealed class AppointmentCreateCommandHandler(
    ICustomerRepository customerRepository,
    AppointmentBookingService bookingService,
    IAppointmentNotificationService notificationService,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork) : IRequestHandler<AppointmentCreateCommand, Result<string>>
{
    // içeri girmiş müşterinin randevusu panelden birkaç dakika geriden girilebilsin
    private static readonly TimeSpan PastTolerance = TimeSpan.FromMinutes(15);

    public async Task<Result<string>> Handle(AppointmentCreateCommand request, CancellationToken cancellationToken)
    {
        var isCustomerExists = await customerRepository.AnyAsync(
            c => c.Id == request.CustomerId && !c.IsAnonymized, cancellationToken);
        if (!isCustomerExists)
        {
            return Result<string>.Failure(404, "Müşteri bulunamadı");
        }

        Note? note = string.IsNullOrWhiteSpace(request.Note) ? null : new Note(request.Note.Trim());
        var booking = await bookingService.CreateAsync(
            request.CustomerId,
            request.StaffId,
            request.SalonServiceIds,
            request.StartAt,
            Source.Panel,
            note,
            timeProvider.GetUtcNow(),
            PastTolerance,
            cancellationToken);
        if (!booking.IsSuccessful)
        {
            return Result<string>.Failure(booking.StatusCode, booking.ErrorMessages!);
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<string>.Failure(409, "Aynı anda başka bir randevu kaydedildi, lütfen tekrar deneyin");
        }

        // kayıttan sonra; e-posta gitmese de randevu alınmış olur
        await notificationService.SendConfirmationAsync(booking.Data!.Id, null, cancellationToken);
        return "Randevu oluşturuldu";
    }
}
