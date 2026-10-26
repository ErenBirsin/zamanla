namespace Zamanla.Application.Services;

public interface IMailService
{
    Task SendAsync(string to, string subject, string htmlBody, string textBody, CancellationToken cancellationToken = default);
}
