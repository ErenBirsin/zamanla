using Zamanla.Domain.TimeOffs;
using Zamanla.Infrastructure.Abstractions;
using Zamanla.Infrastructure.Context;

namespace Zamanla.Infrastructure.Repositories;

internal sealed class TimeOffRepository : AuditableRepository<TimeOff, ApplicationDbContext>, ITimeOffRepository
{
    public TimeOffRepository(ApplicationDbContext context) : base(context)
    {
    }
}
