using Microsoft.EntityFrameworkCore;
using Zamanla.Application.Services;
using Zamanla.Domain.Tenants;

namespace Zamanla.WebAPI;

// dakikada bir bütün salonlar için hatırlatması gelen randevulara bakar
public sealed class AppointmentReminderBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    TimeProvider timeProvider,
    ILogger<AppointmentReminderBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(1), timeProvider);
        do
        {
            try
            {
                await SendRemindersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // bir turdaki hata döngüyü durdurmasın
                logger.LogError(ex, "Hatırlatmalar gönderilirken hata oluştu");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task SendRemindersAsync(CancellationToken cancellationToken)
    {
        List<Guid> tenantIds;
        using (var scope = serviceScopeFactory.CreateScope())
        {
            tenantIds = await scope.ServiceProvider.GetRequiredService<ITenantRepository>()
                .Where(t => t.IsActive)
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);
        }

        var now = timeProvider.GetUtcNow();
        foreach (var tenantId in tenantIds)
        {
            // istek olmadığı için salon elle seçilir; tenant filtresi ve TenantInterceptor buna göre çalışır
            using var scope = serviceScopeFactory.CreateScope();
            scope.ServiceProvider.GetRequiredService<ITenantContext>().SetTenantId(tenantId);

            var reminderService = scope.ServiceProvider.GetRequiredService<AppointmentReminderService>();
            var sentCount = await reminderService.SendDueRemindersAsync(now, cancellationToken);
            if (sentCount > 0)
            {
                logger.LogInformation("{TenantId} için {Count} hatırlatma gönderildi", tenantId, sentCount);
            }
        }
    }
}
