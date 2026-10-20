using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Services;
using Zamanla.Application.Staffs;
using Zamanla.Application.Tenants;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.Customers;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Tenants;
using Zamanla.Domain.TimeOffs;

namespace Zamanla.Application.TimeOffs;

// staffId boşsa bütün salon kapalı; tam gün izinde istemci gece yarısından gece yarısına gönderir
public sealed record TimeOffCreateCommand(
    Guid? StaffId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string? Note) : IRequest<Result<string>>;

public sealed class TimeOffCreateCommandValidator : AbstractValidator<TimeOffCreateCommand>
{
    public TimeOffCreateCommandValidator()
    {
        RuleFor(p => p.StartAt)
            .NotEmpty().WithMessage("Başlangıç zamanı seçin");
        RuleFor(p => p.EndAt)
            .NotEmpty().WithMessage("Bitiş zamanı seçin")
            .GreaterThan(p => p.StartAt).WithMessage("Bitiş, başlangıçtan sonra olmalı");
        RuleFor(p => p)
            .Must(p => p.EndAt - p.StartAt <= TimeSpan.FromDays(90)).WithMessage("İzin en fazla 90 gün olabilir");
        RuleFor(p => p.Note)
            .MaximumLength(250).WithMessage("Açıklama en fazla 250 karakter olabilir");
    }
}

internal sealed class TimeOffCreateCommandHandler(
    ITimeOffRepository timeOffRepository,
    IStaffRepository staffRepository,
    IAppointmentRepository appointmentRepository,
    ICustomerRepository customerRepository,
    ITenantRepository tenantRepository,
    ITenantContext tenantContext,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork) : IRequestHandler<TimeOffCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(TimeOffCreateCommand request, CancellationToken cancellationToken)
    {
        if (request.EndAt <= timeProvider.GetUtcNow())
        {
            return Result<string>.Failure(400, "Geçmiş bir tarih için izin girilemez");
        }

        if (request.StaffId is not null)
        {
            var staffExists = await staffRepository.AnyAsync(s => s.Id == request.StaffId, cancellationToken);
            if (!staffExists)
            {
                return Result<string>.Failure(404, "Personel bulunamadı");
            }
        }

        #region Çakışma Kontrolü
        var exists = await timeOffRepository.AnyAsync(t =>
            t.StaffId == request.StaffId
            && t.Time.Start < request.EndAt
            && t.Time.End > request.StartAt, cancellationToken);
        if (exists)
        {
            return Result<string>.Failure(409, "Bu aralıkta zaten bir izin var");
        }

        // salon kapalıysa bütün personelin randevuları etkilenir
        var confirmed = Status.Confirmed.Value;
        var appointments = await appointmentRepository
            .Where(a => (request.StaffId == null || a.StaffId == request.StaffId)
                && a.Status.Value == confirmed
                && a.Time.Start < request.EndAt
                && a.Time.End > request.StartAt)
            .Join(customerRepository.GetAll(), a => a.CustomerId, c => c.Id, (a, c) => new StaffAppointment(
                a.Time.Start,
                a.Time.End,
                a.SalonServices.Select(s => s.SalonServiceId).ToList(),
                c.FirstName.Value + " " + c.LastName.Value))
            .ToListAsync(cancellationToken);
        if (appointments.Count > 0)
        {
            var timeZone = await tenantRepository.GetTimeZoneAsync(tenantContext.GetTenantId(), cancellationToken);
            var action = request.StaffId is null ? "Bu kapalı gün" : "Bu izin";
            return Result<string>.Failure(409, StaffRules.AppointmentConflictMessage(
                action, appointments.OrderBy(a => a.Start).ToList(), timeZone));
        }
        #endregion

        TimeOff timeOff = new(
            request.StaffId,
            new DateTimeRange(request.StartAt, request.EndAt),
            string.IsNullOrWhiteSpace(request.Note) ? null : new Note(request.Note.Trim()));
        timeOffRepository.Add(timeOff);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return request.StaffId is null ? "Kapalı gün kaydedildi" : "İzin kaydedildi";
    }
}
