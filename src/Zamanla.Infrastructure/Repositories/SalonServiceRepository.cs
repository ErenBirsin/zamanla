using Zamanla.Domain.SalonServices;
using Zamanla.Infrastructure.Abstractions;
using Zamanla.Infrastructure.Context;

namespace Zamanla.Infrastructure.Repositories;

internal sealed class SalonServiceRepository : AuditableRepository<SalonService, ApplicationDbContext>, ISalonServiceRepository
{
    public SalonServiceRepository(ApplicationDbContext context) : base(context)
    {
    }
}
