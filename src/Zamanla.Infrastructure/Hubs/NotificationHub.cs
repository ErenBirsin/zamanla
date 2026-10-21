using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Zamanla.Infrastructure.Services;

namespace Zamanla.Infrastructure.Hubs;

// panel bu hub'a bağlanır; sunucu sadece "değişti" der, veriyi client kendisi yeniden çeker
[Authorize]
public sealed class NotificationHub : Hub
{
    public const string CalendarChanged = "calendarChanged";
    public const string NotificationCreated = "notificationCreated";

    public static string GroupName(Guid tenantId) => $"tenant-{tenantId}";

    // bağlanan kullanıcı kendi salonunun grubuna eklenir; başka salonun olayları ona gitmez
    public override async Task OnConnectedAsync()
    {
        var value = Context.User?.FindFirst(ClaimNames.TenantId)?.Value;
        if (Guid.TryParse(value, out var tenantId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(tenantId));
        }

        await base.OnConnectedAsync();
    }
}
