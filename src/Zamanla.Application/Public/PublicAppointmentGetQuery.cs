using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Services;
using Zamanla.Application.Tenants;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.Customers;
using Zamanla.Domain.SalonServices;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Tenants;

namespace Zamanla.Application.Public;

// müşterinin linkten açtığı randevu sayfası
public sealed record PublicAppointmentGetQuery(string Token) : IRequest<Result<PublicAppointmentDto>>;

public sealed class PublicAppointmentServiceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
}

public sealed class PublicAppointmentDto
{
    public string AppointmentNumber { get; set; } = default!;
    public string CustomerFullName { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public Guid StaffId { get; set; }
    public string StaffFullName { get; set; } = default!;
    public List<PublicAppointmentServiceDto> Services { get; set; } = new();
    public decimal Total { get; set; }
    public bool CanCancel { get; set; }
    public bool CanReschedule { get; set; }
    public DateTimeOffset CancelDeadline { get; set; }
}

internal sealed class PublicAppointmentGetQueryHandler(
    IAppointmentRepository appointmentRepository,
    ICustomerRepository customerRepository,
    IStaffRepository staffRepository,
    ISalonServiceRepository salonServiceRepository,
    ITenantRepository tenantRepository,
    ITenantContext tenantContext,
    TimeProvider timeProvider) : IRequestHandler<PublicAppointmentGetQuery, Result<PublicAppointmentDto>>
{
    public async Task<Result<PublicAppointmentDto>> Handle(PublicAppointmentGetQuery request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByManageTokenAsync(request.Token, cancellationToken);
        if (appointment is null)
        {
            return Result<PublicAppointmentDto>.Failure(404, "Randevu bulunamadı");
        }

        var customerFullName = await customerRepository
            .Where(c => c.Id == appointment.CustomerId)
            .Select(c => c.FirstName.Value + " " + c.LastName.Value)
            .FirstAsync(cancellationToken);
        // kapanmış randevunun personeli ya da hizmeti sonradan silinmiş olabilir
        var staffFullName = await staffRepository.GetAllIncludingDeleted()
            .Where(s => s.Id == appointment.StaffId)
            .Select(s => s.FirstName.Value + " " + s.LastName.Value)
            .FirstAsync(cancellationToken);
        var salonServiceIds = appointment.SalonServices.Select(s => s.SalonServiceId).ToList();
        var serviceNames = await salonServiceRepository.GetAllIncludingDeleted()
            .Where(s => salonServiceIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name.Value, cancellationToken);

        var now = timeProvider.GetUtcNow();
        var isOpen = appointment.Status == Status.Confirmed;
        var policy = await tenantRepository.GetBookingPolicyAsync(tenantContext.GetTenantId(), cancellationToken);
        var cancelDeadline = appointment.Time.Start.AddHours(-policy.CancelHoursBefore);

        return new PublicAppointmentDto
        {
            AppointmentNumber = appointment.AppointmentNumber.Value,
            CustomerFullName = customerFullName,
            Status = appointment.Status.Value,
            StartAt = appointment.Time.Start,
            EndAt = appointment.Time.End,
            StaffId = appointment.StaffId,
            StaffFullName = staffFullName,
            Services = salonServiceIds
                .Select(id => new PublicAppointmentServiceDto { Id = id, Name = serviceNames[id] })
                .ToList(),
            Total = appointment.Total.Value,
            CanCancel = isOpen && now < cancelDeadline,
            CanReschedule = isOpen && now < cancelDeadline,
            CancelDeadline = cancelDeadline
        };
    }
}
