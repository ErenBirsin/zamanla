using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Zamanla.Application.Services;
using Zamanla.Domain.Abstractions;

namespace Zamanla.Infrastructure.Interceptors;

internal sealed class TenantInterceptor(ITenantContext tenantContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyTenantRules(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyTenantRules(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyTenantRules(DbContext? context)
    {
        var tenantId = tenantContext.GetTenantId();

        // kayıt ve giriş gibi oturumsuz işlemlerde kontrol yok
        if (context is null || tenantId is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State is EntityState.Unchanged or EntityState.Detached)
            {
                continue;
            }

            if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
            {
                entry.Property(nameof(ITenantEntity.TenantId)).CurrentValue = tenantId.Value;
                continue;
            }

            if (entry.Entity.TenantId != tenantId.Value)
            {
                throw new InvalidOperationException(
                    $"{entry.Entity.GetType().Name} başka bir işletmeye ait, değiştirilemez.");
            }
        }
    }
}
