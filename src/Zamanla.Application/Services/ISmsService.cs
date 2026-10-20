namespace Zamanla.Application.Services;

// gerçek bir SMS sağlayıcısı seçilene kadar mesajlar log'a yazılır
public interface ISmsService
{
    Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}
