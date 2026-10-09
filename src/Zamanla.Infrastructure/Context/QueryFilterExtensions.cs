using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Zamanla.Domain.Abstractions;

namespace Zamanla.Infrastructure.Context;
internal static class QueryFilterExtensions
{
    public const string SoftDeleteFilter = "SoftDelete";

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
}
