using Zamanla.Domain.Abstractions;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Customers;
using Zamanla.Domain.SalonServices;
using Zamanla.Domain.Staffs;

namespace Zamanla.Application.Appointments;

public sealed class AppointmentSalonServiceDto
{
    public Guid SalonServiceId { get; set; }
    public string Name { get; set; } = default!;
    public int Duration { get; set; }
    public decimal Price { get; set; }
}

public sealed class AppointmentHistoryDto
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class AppointmentDto : EntityDto
{
    public string AppointmentNumber { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public string CustomerFullName { get; set; } = default!;
    public string? CustomerPhoneNumber { get; set; }
    // 3 ve üstünde takvimde uyarı gösterilecek
    public int CustomerNoShowCount { get; set; }
    public Guid StaffId { get; set; }
    public string StaffFullName { get; set; } = default!;
    public string StaffColor { get; set; } = default!;
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public DateTimeOffset BlockedUntil { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = default!;
    public string Source { get; set; } = default!;
    public string? Note { get; set; }
    public List<AppointmentSalonServiceDto> SalonServices { get; set; } = new();
    public List<AppointmentHistoryDto> Histories { get; set; } = new();
}

public static class AppointmentExtensions
{
    public static IQueryable<AppointmentDto> MapTo(
        this IQueryable<EntityWithAuditDto<Appointment>> entities,
        IQueryable<Customer> customers,
        IQueryable<Staff> staffs,
        IQueryable<SalonService> salonServices)
    {
        return entities
            .Join(customers, m => m.Entity.CustomerId, m => m.Id, (a, customer) => new
            {
                a.Entity,
                a.CreatedUser,
                a.UpdatedUser,
                Customer = customer
            })
            .Join(staffs, m => m.Entity.StaffId, m => m.Id, (a, staff) => new
            {
                a.Entity,
                a.CreatedUser,
                a.UpdatedUser,
                a.Customer,
                Staff = staff
            })
            .Select(s => new AppointmentDto
            {
                Id = s.Entity.Id,
                AppointmentNumber = s.Entity.AppointmentNumber.Value,
                CustomerId = s.Entity.CustomerId,
                CustomerFullName = s.Customer.FirstName.Value + " " + s.Customer.LastName.Value,
                CustomerPhoneNumber = s.Customer.PhoneNumber != null ? s.Customer.PhoneNumber.Value : null,
                CustomerNoShowCount = s.Customer.NoShowCount,
                StaffId = s.Entity.StaffId,
                StaffFullName = s.Staff.FirstName.Value + " " + s.Staff.LastName.Value,
                StaffColor = s.Staff.Color.Value,
                StartAt = s.Entity.Time.Start,
                EndAt = s.Entity.Time.End,
                BlockedUntil = s.Entity.BlockedUntil,
                Total = s.Entity.Total.Value,
                Status = s.Entity.Status.Value,
                Source = s.Entity.Source.Value,
                Note = s.Entity.Note != null ? s.Entity.Note.Value : null,
                SalonServices = s.Entity.SalonServices
                    .Join(salonServices, m => m.SalonServiceId, m => m.Id, (appointmentService, salonService) => new AppointmentSalonServiceDto
                    {
                        SalonServiceId = appointmentService.SalonServiceId,
                        Name = salonService.Name.Value,
                        Duration = appointmentService.Duration,
                        Price = appointmentService.Price
                    })
                    .ToList(),
                Histories = s.Entity.Histories
                    .OrderBy(h => h.CreatedAt)
                    .Select(h => new AppointmentHistoryDto
                    {
                        Title = h.Title,
                        Description = h.Description,
                        CreatedAt = h.CreatedAt
                    })
                    .ToList(),
                CreatedAt = s.Entity.CreatedAt,
                CreatedBy = s.Entity.CreatedBy,
                CreatedFullName = s.CreatedUser != null
                    ? s.CreatedUser.FirstName.Value + " " + s.CreatedUser.LastName.Value
                    : null,
                UpdatedAt = s.Entity.UpdatedAt,
                UpdatedBy = s.Entity.UpdatedBy,
                UpdatedFullName = s.UpdatedUser != null
                    ? s.UpdatedUser.FirstName.Value + " " + s.UpdatedUser.LastName.Value
                    : null,
            });
    }
}
