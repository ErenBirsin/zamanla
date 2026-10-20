namespace Zamanla.Application.Services;

// gönderilemeyen bildirim randevuyu etkilemez, hata log'a yazılır
public interface IAppointmentNotificationService
{
    // manageToken yalnızca online randevuda ve oluşturulduğu istekte bilinir
    Task SendConfirmationAsync(Guid appointmentId, string? manageToken, CancellationToken cancellationToken = default);
}
