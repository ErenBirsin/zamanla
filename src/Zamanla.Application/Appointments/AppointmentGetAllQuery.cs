using TS.MediatR;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Customers;
using Zamanla.Domain.SalonServices;
using Zamanla.Domain.Staffs;

namespace Zamanla.Application.Appointments;

public sealed record AppointmentGetAllQuery : IRequest<IQueryable<AppointmentDto>>;

internal sealed class AppointmentGetAllQueryHandler(
    IAppointmentRepository appointmentRepository,
    ICustomerRepository customerRepository,
    IStaffRepository staffRepository,
    ISalonServiceRepository salonServiceRepository) : IRequestHandler<AppointmentGetAllQuery, IQueryable<AppointmentDto>>
{
    public Task<IQueryable<AppointmentDto>> Handle(AppointmentGetAllQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(appointmentRepository.GetAllWithAudit()
            .MapTo(
                customerRepository.GetAll(),
                staffRepository.GetAllIncludingDeleted(),
                salonServiceRepository.GetAllIncludingDeleted()));
}
