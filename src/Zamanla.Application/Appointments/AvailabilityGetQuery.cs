using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Services;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.SalonServices;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Tenants;

namespace Zamanla.Application.Appointments;

// staffId boşsa fark etmez: hizmetleri veren tüm personel
public sealed record AvailabilityGetQuery(
    DateOnly Date,
    List<Guid> SalonServiceIds,
    Guid? StaffId) : IRequest<Result<List<AvailabilitySlotDto>>>;

public sealed class AvailabilitySlotDto
{
    public DateTimeOffset StartAt { get; set; }
    public Guid StaffId { get; set; }
    public string StaffFullName { get; set; } = default!;
}

public sealed class AvailabilityGetQueryValidator : AbstractValidator<AvailabilityGetQuery>
{
    public AvailabilityGetQueryValidator()
    {
        RuleFor(p => p.Date)
            .NotEmpty().WithMessage("Tarih seçin");
        RuleFor(p => p.SalonServiceIds)
            .NotEmpty().WithMessage("En az bir hizmet seçin");
    }
}

internal sealed class AvailabilityGetQueryHandler(
    ISalonServiceRepository salonServiceRepository,
    IStaffRepository staffRepository,
    IAppointmentRepository appointmentRepository,
    ITenantRepository tenantRepository,
    ITenantContext tenantContext,
    TimeProvider timeProvider) : IRequestHandler<AvailabilityGetQuery, Result<List<AvailabilitySlotDto>>>
{
    public async Task<Result<List<AvailabilitySlotDto>>> Handle(AvailabilityGetQuery request, CancellationToken cancellationToken)
    {
        #region Hizmet ve Personel
        var salonServiceIds = request.SalonServiceIds.Distinct().ToList();
        var salonServices = await salonServiceRepository
            .Where(s => salonServiceIds.Contains(s.Id) && s.IsActive)
            .ToListAsync(cancellationToken);
        if (salonServices.Count != salonServiceIds.Count)
        {
            return Result<List<AvailabilitySlotDto>>.Failure(404, "Seçilen hizmetlerden biri bulunamadı ya da pasif");
        }

        var duration = salonServices.Sum(s => s.Duration.Value);
        var buffer = salonServices.Max(s => s.Buffer.Value);

        var staffs = await staffRepository
            .Where(s => s.IsActive && (request.StaffId == null || s.Id == request.StaffId))
            .ToListAsync(cancellationToken);
        // yalnızca seçilen hizmetlerin hepsini veren personel
        staffs = staffs
            .Where(staff => salonServiceIds.All(id => staff.SalonServices.Any(s => s.SalonServiceId == id)))
            .ToList();
        if (request.StaffId is not null && staffs.Count == 0)
        {
            return Result<List<AvailabilitySlotDto>>.Failure(400, "Seçilen personel bu hizmetler için randevu alamıyor");
        }
        #endregion

        #region Gün ve Dolu Saatler
        var tenantId = tenantContext.GetTenantId();
        var timeZoneId = await tenantRepository
            .Where(t => t.Id == tenantId)
            .Select(t => t.TimeZoneId.Value)
            .FirstAsync(cancellationToken);
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        // işletmenin saat diliminde günün başı ve sonu
        var localMidnight = request.Date.ToDateTime(TimeOnly.MinValue);
        var dayStart = new DateTimeOffset(localMidnight, timeZone.GetUtcOffset(localMidnight));
        var dayEnd = dayStart.AddDays(1);

        var staffIds = staffs.Select(s => s.Id).ToList();
        var confirmed = Status.Confirmed.Value;
        var busy = await appointmentRepository
            .Where(a => staffIds.Contains(a.StaffId)
                && a.Status.Value == confirmed
                && a.Time.Start < dayEnd
                && a.BlockedUntil > dayStart)
            .Select(a => new { a.StaffId, a.Time.Start, a.BlockedUntil })
            .ToListAsync(cancellationToken);
        #endregion

        var now = timeProvider.GetUtcNow();
        var slots = new List<AvailabilitySlotDto>();
        foreach (var staff in staffs)
        {
            var staffBusy = busy
                .Where(b => b.StaffId == staff.Id)
                .Select(b => new DateTimeRange(b.Start, b.BlockedUntil));
            var staffSlots = AvailabilityCalculator.GetSlots(
                request.Date, timeZone, staff.WorkingHours, staffBusy, duration, buffer, now);

            slots.AddRange(staffSlots.Select(startAt => new AvailabilitySlotDto
            {
                StartAt = startAt,
                StaffId = staff.Id,
                StaffFullName = staff.FullName
            }));
        }

        // aynı saatte birden fazla personel boşsa o gün en az randevusu olan seçilir
        var appointmentCounts = busy
            .GroupBy(b => b.StaffId)
            .ToDictionary(g => g.Key, g => g.Count());
        var result = slots
            .GroupBy(s => s.StartAt)
            .Select(g => g.OrderBy(s => appointmentCounts.GetValueOrDefault(s.StaffId)).First())
            .OrderBy(s => s.StartAt)
            .ToList();

        return result;
    }
}
