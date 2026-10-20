using Microsoft.Extensions.Logging;
using Zamanla.Application.Services;

namespace Zamanla.Infrastructure.Services;

internal sealed class FakeSmsService(ILogger<FakeSmsService> logger) : ISmsService
{
    public Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        // numaranın tamamı log'a yazılmaz: +90532*****67
        var masked = phoneNumber.Length > 8 ? $"{phoneNumber[..6]}*****{phoneNumber[^2..]}" : "***";
        logger.LogInformation("SMS gönderildi (sahte) {PhoneNumber}: {Message}", masked, message);
        return Task.CompletedTask;
    }
}
