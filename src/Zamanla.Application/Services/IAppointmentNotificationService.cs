namespace Zamanla.Application.Services;

// gönderilemeyen bildirim randevuyu etkilemez, hata log'a yazılır
public interface IAppointmentNotificationService
{
    // manageToken yalnızca online randevuda ve oluşturulduğu istekte bilinir
    Task SendConfirmationAsync(Guid appointmentId, string? manageToken, CancellationToken cancellationToken = default);

    // e-posta gönderilemezse false döner, hatırlatma bir sonraki turda tekrar denenir
    Task<bool> SendReminderAsync(Guid appointmentId, CancellationToken cancellationToken = default);
}
