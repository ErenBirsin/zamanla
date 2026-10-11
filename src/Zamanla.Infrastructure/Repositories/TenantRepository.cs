using GenericRepository;
using Zamanla.Domain.Tenants;
using Zamanla.Infrastructure.Context;

namespace Zamanla.Infrastructure.Repositories;
internal sealed class TenantRepository(ApplicationDbContext context)
    : Repository<Tenant, ApplicationDbContext>(context), ITenantRepository;
