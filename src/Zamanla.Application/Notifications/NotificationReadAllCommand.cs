using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;
using Zamanla.Domain.Notifications;

namespace Zamanla.Application.Notifications;

public sealed record NotificationReadAllCommand : IRequest<Result<string>>;

internal sealed class NotificationReadAllCommandHandler(
    INotificationRepository notificationRepository) : IRequestHandler<NotificationReadAllCommand, Result<string>>
{
    public async Task<Result<string>> Handle(NotificationReadAllCommand request, CancellationToken cancellationToken)
    {
        // kayıtları belleğe çekmeden tek sorguyla; tenant filtresi Where'de de geçerli
        await notificationRepository
            .Where(n => !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken);

        return "Bildirimler okundu";
    }
}
