using GenericRepository;
using Zamanla.Domain.Tenants.ValueObjects;

namespace Zamanla.Domain.Tenants;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<bool> SlugExistsAsync(Slug slug, CancellationToken cancellationToken = default);
}
