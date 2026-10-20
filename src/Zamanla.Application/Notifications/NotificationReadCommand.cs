using GenericRepository;
using TS.MediatR;
using TS.Result;
using Zamanla.Domain.Notifications;

namespace Zamanla.Application.Notifications;

public sealed record NotificationReadCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class NotificationReadCommandHandler(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<NotificationReadCommand, Result<string>>
{
    public async Task<Result<string>> Handle(NotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);
        if (notification is null)
        {
            return Result<string>.Failure(404, "Bildirim bulunamadı");
        }

        notification.MarkAsRead();
        notificationRepository.Update(notification);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Bildirim okundu";
    }
}
