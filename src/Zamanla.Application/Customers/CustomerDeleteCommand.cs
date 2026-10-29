using System.Globalization;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Behaviors;
using Zamanla.Application.Permissions;
using Zamanla.Application.Services;
using Zamanla.Application.Tenants;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Appointments.ValueObjects;
using Zamanla.Domain.Customers;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Tenants;

namespace Zamanla.Application.Customers;

[Permission(PermissionCatalog.CustomersDelete)]
public sealed record CustomerDeleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class CustomerDeleteCommandHandler(
    ICustomerRepository customerRepository,
    IAppointmentRepository appointmentRepository,
    IStaffRepository staffRepository,
    ITenantRepository tenantRepository,
    ITenantContext tenantContext,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork) : IRequestHandler<CustomerDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CustomerDeleteCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.FirstOrDefaultAsync(
            c => c.Id == request.Id && !c.IsAnonymized,
            cancellationToken);
        if (customer is null)
        {
            return Result<string>.Failure(404, "Müşteri bulunamadı");
        }

        // ileri tarihli randevusu olan müşteri silinirse randevu isimsiz kalır, salon ona ulaşamaz
        var now = timeProvider.GetUtcNow();
        var confirmed = Status.Confirmed.Value;
        var upcoming = await appointmentRepository
            .Where(a => a.CustomerId == customer.Id && a.Status.Value == confirmed && a.Time.Start >= now)
            .Join(staffRepository.GetAllIncludingDeleted(), a => a.StaffId, s => s.Id, (a, s) => new
            {
                a.Time.Start,
                StaffFullName = s.FirstName.Value + " " + s.LastName.Value
            })
            .ToListAsync(cancellationToken);
        if (upcoming.Count > 0)
        {
            var timeZone = await tenantRepository.GetTimeZoneAsync(tenantContext.GetTenantId(), cancellationToken);
            var culture = CultureInfo.GetCultureInfo("tr-TR");
            var listed = upcoming
                .OrderBy(a => a.Start)
                .Take(3)
                .Select(a => $"{TimeZoneInfo.ConvertTime(a.Start, timeZone).ToString("d MMMM HH:mm", culture)} {a.StaffFullName}");

            var message = $"Müşterinin {upcoming.Count} ileri tarihli randevusu var. "
                + $"Silmeden önce bu randevuları iptal edin: {string.Join(", ", listed)}";
            if (upcoming.Count > 3)
            {
                message += $" ve {upcoming.Count - 3} randevu daha";
            }

            return Result<string>.Failure(409, message);
        }

        // kayıt silinmez, kişisel bilgiler temizlenir
        customer.Anonymize();
        customerRepository.Update(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Müşteri silindi";
    }
}
