using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Notifications;
using Zamanla.Domain.TimeOffs;
using Zamanla.Infrastructure.Hubs;

namespace Zamanla.Infrastructure.Interceptors;

// randevu ya da izin değişince ve yeni bildirim eklenince salonun açık panellerine haber verir;
// her komuta ayrı satır yazmak yerine kayıt anında tek yerden
internal sealed class RealtimeInterceptor(
    IHubContext<NotificationHub> hubContext,
    ILogger<RealtimeInterceptor> logger) : SaveChangesInterceptor
{
    private readonly List<Guid> _calendarTenantIds = [];
    private readonly List<Guid> _notificationTenantIds = [];

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Collect(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await PublishAsync(cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        // kayıt olmadıysa haber de verilmez
        _calendarTenantIds.Clear();
        _notificationTenantIds.Clear();
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    // kayıttan sonra hepsi Unchanged olur, silinenler takipten çıkar; o yüzden kayıttan önce toplanır.
    // TenantInterceptor'dan sonra çalışmalı ki yeni kayıtların TenantId'si dolmuş olsun
    private void Collect(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var tracker = context.ChangeTracker;
        _calendarTenantIds.AddRange(tracker.Entries<Appointment>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .Select(e => e.Entity.TenantId));
        _calendarTenantIds.AddRange(tracker.Entries<TimeOff>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => e.Entity.TenantId));
        _notificationTenantIds.AddRange(tracker.Entries<Notification>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity.TenantId));
    }

    private async Task PublishAsync(CancellationToken cancellationToken)
    {
        var calendarTenantIds = _calendarTenantIds.Distinct().ToList();
        var notificationTenantIds = _notificationTenantIds.Distinct().ToList();
        _calendarTenantIds.Clear();
        _notificationTenantIds.Clear();

        // kayıt tamamlandı; haber verilemezse istek bozulmaz, panel bir sonraki yenilemede görür
        try
        {
            foreach (var tenantId in calendarTenantIds)
            {
                await hubContext.Clients.Group(NotificationHub.GroupName(tenantId))
                    .SendAsync(NotificationHub.CalendarChanged, cancellationToken);
            }

            foreach (var tenantId in notificationTenantIds)
            {
                await hubContext.Clients.Group(NotificationHub.GroupName(tenantId))
                    .SendAsync(NotificationHub.NotificationCreated, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Canlı güncelleme gönderilemedi");
        }
    }
}
