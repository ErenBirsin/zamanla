using GenericRepository;
using Zamanla.Domain.Notifications;
using Zamanla.Infrastructure.Context;

namespace Zamanla.Infrastructure.Repositories;

internal sealed class NotificationRepository : Repository<Notification, ApplicationDbContext>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context)
    {
    }
}
