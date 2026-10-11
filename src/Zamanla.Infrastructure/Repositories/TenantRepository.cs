using GenericRepository;
using Zamanla.Domain.Tenants;
using Zamanla.Infrastructure.Context;

namespace Zamanla.Infrastructure.Repositories;

internal sealed class TenantRepository : Repository<Tenant, ApplicationDbContext>, ITenantRepository
{
    public TenantRepository(ApplicationDbContext context) : base(context)
    {
    }
}
