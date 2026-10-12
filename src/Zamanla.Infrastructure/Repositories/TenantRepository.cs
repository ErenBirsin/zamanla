using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Zamanla.Domain.Tenants;
using Zamanla.Domain.Tenants.ValueObjects;
using Zamanla.Infrastructure.Context;

namespace Zamanla.Infrastructure.Repositories;

internal sealed class TenantRepository : Repository<Tenant, ApplicationDbContext>, ITenantRepository
{
    private readonly ApplicationDbContext _context;

    public TenantRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<bool> SlugExistsAsync(Slug slug, CancellationToken cancellationToken = default)
    {
        return _context.Set<Tenant>()
            .IgnoreQueryFilters([QueryFilterExtensions.SoftDeleteFilter])
            .AnyAsync(t => t.Slug.Value == slug.Value, cancellationToken);
    }
}
