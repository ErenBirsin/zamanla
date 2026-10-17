using Zamanla.Domain.Abstractions;

namespace Zamanla.Domain.Appointments;

public interface IAppointmentRepository : IAuditableRepository<Appointment>
{
}
