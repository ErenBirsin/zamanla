using TS.MediatR;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Customers;
using Zamanla.Domain.Notifications;

namespace Zamanla.Application.Notifications;

public sealed record NotificationGetAllQuery : IRequest<IQueryable<NotificationDto>>;

public sealed class NotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = default!;
    public Guid AppointmentId { get; set; }
    public string CustomerFullName { get; set; } = default!;
    public DateTimeOffset AppointmentStartAt { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

internal sealed class NotificationGetAllQueryHandler(
    INotificationRepository notificationRepository,
    IAppointmentRepository appointmentRepository,
    ICustomerRepository customerRepository) : IRequestHandler<NotificationGetAllQuery, IQueryable<NotificationDto>>
{
    public Task<IQueryable<NotificationDto>> Handle(NotificationGetAllQuery request, CancellationToken cancellationToken)
    {
        // ad ve saat randevudan okunur; müşteri silindiyse "Silinmiş Müşteri" görünür
        var notifications =
            from notification in notificationRepository.GetAll()
            join appointment in appointmentRepository.GetAll() on notification.AppointmentId equals appointment.Id
            join customer in customerRepository.GetAll() on appointment.CustomerId equals customer.Id
            select new NotificationDto
            {
                Id = notification.Id,
                Type = notification.Type.Value,
                AppointmentId = notification.AppointmentId,
                CustomerFullName = customer.FirstName.Value + " " + customer.LastName.Value,
                AppointmentStartAt = appointment.Time.Start,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };

        return Task.FromResult(notifications);
    }
}
