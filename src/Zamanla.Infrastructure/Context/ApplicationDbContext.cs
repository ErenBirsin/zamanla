using Microsoft.EntityFrameworkCore;

namespace Zamanla.Infrastructure.Context;

internal sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext>options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.ApplySoftDeleteFilter();
    }
}
