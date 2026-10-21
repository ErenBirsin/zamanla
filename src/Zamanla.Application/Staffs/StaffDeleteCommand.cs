using GenericRepository;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Application.Behaviors;
using Zamanla.Application.Permissions;
using Zamanla.Application.Services;
using Zamanla.Application.Tenants;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Customers;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Tenants;

namespace Zamanla.Application.Staffs;

[Permission(PermissionCatalog.StaffManage)]
public sealed record StaffDeleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class StaffDeleteCommandHandler(
    IStaffRepository staffRepository,
    IAppointmentRepository appointmentRepository,
    ICustomerRepository customerRepository,
    ITenantRepository tenantRepository,
    ITenantContext tenantContext,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork) : IRequestHandler<StaffDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(StaffDeleteCommand request, CancellationToken cancellationToken)
    {
        var staff = await staffRepository.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (staff is null)
        {
            return Result<string>.Failure(404, "Personel bulunamadı");
        }

        var upcoming = await StaffRules.GetUpcomingAppointmentsAsync(
            appointmentRepository, customerRepository, staff.Id, timeProvider.GetUtcNow(), cancellationToken);
        if (upcoming.Count > 0)
        {
            var timeZone = await tenantRepository.GetTimeZoneAsync(tenantContext.GetTenantId(), cancellationToken);
            return Result<string>.Failure(409, StaffRules.AppointmentConflictMessage("Personeli silmek", upcoming, timeZone));
        }

        staff.Delete();
        staffRepository.Update(staff);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<string>.Failure(409, "Bu personele aynı anda randevu kaydedildi, lütfen tekrar deneyin");
        }

        return "Personel silindi";
    }
}
