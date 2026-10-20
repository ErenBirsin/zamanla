using Microsoft.EntityFrameworkCore;
using Zamanla.Domain.RefreshTokens;

namespace Zamanla.WebAPI;

// süresi dolan refresh token'lar günde bir silinir; süresi dolmamış pasif token'lar kalır,
// tekrar kullanım tespiti onlara bakıyor
public sealed class RefreshTokenCleanupBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    TimeProvider timeProvider,
    ILogger<RefreshTokenCleanupBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromDays(1), timeProvider);
        do
        {
            try
            {
                await DeleteExpiredAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Süresi dolmuş refresh token'lar silinemedi");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task DeleteExpiredAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var refreshTokenRepository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

        var now = timeProvider.GetUtcNow();
        var deletedCount = await refreshTokenRepository
            .Where(t => t.ExpiresDate.Value < now)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedCount > 0)
        {
            logger.LogInformation("{Count} süresi dolmuş refresh token silindi", deletedCount);
        }
    }
}
