using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Customers;
using Zamanla.Domain.SalonServices;
using Zamanla.Domain.Staffs;

namespace Zamanla.Application.Appointments;

public sealed record AppointmentGetQuery(Guid Id) : IRequest<Result<AppointmentDto>>;

internal sealed class AppointmentGetQueryHandler(
    IAppointmentRepository appointmentRepository,
    ICustomerRepository customerRepository,
    IStaffRepository staffRepository,
    ISalonServiceRepository salonServiceRepository) : IRequestHandler<AppointmentGetQuery, Result<AppointmentDto>>
{
    public async Task<Result<AppointmentDto>> Handle(AppointmentGetQuery request, CancellationToken cancellationToken)
    {
        var res = await appointmentRepository
            .GetAllWithAudit()
            .MapTo(
                customerRepository.GetAll(),
                staffRepository.GetAllIncludingDeleted(),
                salonServiceRepository.GetAllIncludingDeleted())
            .Where(a => a.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (res is null)
        {
            return Result<AppointmentDto>.Failure(404, "Randevu bulunamadı");
        }

        return res;
    }
}
