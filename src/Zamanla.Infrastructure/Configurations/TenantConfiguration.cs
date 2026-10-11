using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zamanla.Domain.Tenants;

namespace Zamanla.Infrastructure.Configurations;
internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);

        builder.OwnsOne(t => t.Name, name =>
        {
            name.Property(n => n.Value).HasColumnName("Name").HasMaxLength(100);
        });

        builder.OwnsOne(t => t.Slug, slug =>
        {
            slug.Property(s => s.Value).HasColumnName("Slug").HasMaxLength(100);
            slug.HasIndex(s => s.Value).IsUnique();
        });

        builder.OwnsOne(t => t.TimeZoneId, timeZone =>
        {
            timeZone.Property(z => z.Value).HasColumnName("TimeZoneId").HasMaxLength(100);
        });

        builder.Navigation(t => t.Name).IsRequired();
        builder.Navigation(t => t.Slug).IsRequired();
        builder.Navigation(t => t.TimeZoneId).IsRequired();
    }
}
