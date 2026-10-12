using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zamanla.Domain.SalonServices;
using Zamanla.Domain.Tenants;

namespace Zamanla.Infrastructure.Configurations;

internal sealed class SalonServiceConfiguration : IEntityTypeConfiguration<SalonService>
{
    public void Configure(EntityTypeBuilder<SalonService> builder)
    {
        builder.ToTable("SalonServices");
        builder.HasKey(s => s.Id);

        builder.OwnsOne(s => s.Name, name =>
        {
            name.Property(n => n.Value).HasColumnName("Name").HasMaxLength(100);
        });

        builder.OwnsOne(s => s.Category, category =>
        {
            category.Property(c => c.Value).HasColumnName("Category").HasMaxLength(100);
        });

        builder.OwnsOne(s => s.Duration, duration =>
        {
            duration.Property(d => d.Value).HasColumnName("DurationMinutes");
        });

        builder.OwnsOne(s => s.Buffer, buffer =>
        {
            buffer.Property(b => b.Value).HasColumnName("BufferMinutes");
        });

        builder.OwnsOne(s => s.Price, price =>
        {
            price.Property(p => p.Value).HasColumnName("Price").HasPrecision(18, 2);
        });

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(s => s.Name).IsRequired();
        builder.Navigation(s => s.Category).IsRequired();
        builder.Navigation(s => s.Duration).IsRequired();
        builder.Navigation(s => s.Buffer).IsRequired();
        builder.Navigation(s => s.Price).IsRequired();
    }
}
