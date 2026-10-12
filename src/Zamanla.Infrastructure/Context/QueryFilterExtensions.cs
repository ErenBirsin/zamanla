using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Zamanla.Domain.Abstractions;

namespace Zamanla.Infrastructure.Context;
internal static class QueryFilterExtensions
{
    public const string SoftDeleteFilter = "SoftDelete";
    public const string TenantFilter = "Tenant";

    public static void ApplySoftDeleteFilter(this ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(SoftDeletableEntity).IsAssignableFrom(t.ClrType) && t.BaseType is null)
            .ToList();

        foreach (var entityType in entityTypes)
        {
            // Her soft deleteli entity için şu lambdayı kod ile üretiyoruz: e => !e.IsDeleted
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var isDeleted = Expression.Property(parameter, nameof(SoftDeletableEntity.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(isDeleted), parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(SoftDeleteFilter, filter);
        }
    }

    public static void ApplyTenantFilter(this ModelBuilder modelBuilder, ApplicationDbContext context)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(ITenantEntity).IsAssignableFrom(t.ClrType) && t.BaseType is null)
            .ToList();

        foreach (var entityType in entityTypes)
        {
            // e => e.TenantId == context.CurrentTenantId
            var parameter = Expression.Parameter(entityType.ClrType, "e");

            var tenantId = Expression.Convert(
                Expression.Property(parameter, nameof(ITenantEntity.TenantId)),
                typeof(Guid?));

            var currentTenantId = Expression.Property(
                Expression.Constant(context),
                nameof(ApplicationDbContext.CurrentTenantId));

            var filter = Expression.Lambda(Expression.Equal(tenantId, currentTenantId), parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(TenantFilter, filter);
        }
    }
}
