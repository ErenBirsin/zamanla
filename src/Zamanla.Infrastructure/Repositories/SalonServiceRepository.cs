using GenericRepository;
using Zamanla.Domain.SalonServices;
using Zamanla.Infrastructure.Context;

namespace Zamanla.Infrastructure.Repositories;
internal sealed class SalonServiceRepository : Repository<SalonService, ApplicationDbContext>, ISalonServiceRepository
{
    public SalonServiceRepository(ApplicationDbContext context) : base(context)
    {
    }
}
