using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Zamanla.Application.Services;

namespace Zamanla.Infrastructure.Context;

internal sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ITenantContext tenantContext) : DbContext(options), IUnitOfWork
{
    // tenant filtresi her sorguda bu değeri okur
    public Guid? CurrentTenantId => tenantContext.GetTenantId();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.ApplySoftDeleteFilter();
        modelBuilder.ApplyTenantFilter(this);
    }
}
