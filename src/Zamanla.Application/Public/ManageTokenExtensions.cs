using Zamanla.Domain.Appointments;
using Zamanla.Domain.Appointments.ValueObjects;

namespace Zamanla.Application.Public;

public static class ManageTokenExtensions
{
    // linkteki token'ın özeti hesaplanır, randevu özetle bulunur
    public static async Task<Appointment?> GetByManageTokenAsync(
        this IAppointmentRepository appointmentRepository,
        string token,
        CancellationToken cancellationToken = default)
    {
        var hash = ManageToken.FromPlainText(token).Value;
        return await appointmentRepository.FirstOrDefaultAsync(a => a.ManageToken!.Value == hash, cancellationToken);
    }
}
