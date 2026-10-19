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

        builder.OwnsOne(t => t.Address, address =>
        {
            address.Property(a => a.City).HasColumnName("City").HasMaxLength(50);
            address.Property(a => a.District).HasColumnName("District").HasMaxLength(50);
            address.Property(a => a.FullAddress).HasColumnName("FullAddress").HasMaxLength(250);
        });

        builder.OwnsOne(t => t.PhoneNumber, phoneNumber =>
        {
            phoneNumber.Property(p => p.Value).HasColumnName("PhoneNumber").HasMaxLength(13);
        });

        builder.OwnsOne(t => t.BookingPolicy, policy =>
        {
            policy.Property(p => p.MaxDaysAhead).HasColumnName("BookingMaxDaysAhead");
            policy.Property(p => p.CancelHoursBefore).HasColumnName("BookingCancelHoursBefore");
            policy.Property(p => p.MaxUpcomingPerPhone).HasColumnName("BookingMaxUpcomingPerPhone");
        });

        builder.Navigation(t => t.Name).IsRequired();
        builder.Navigation(t => t.Slug).IsRequired();
        builder.Navigation(t => t.TimeZoneId).IsRequired();
        builder.Navigation(t => t.BookingPolicy).IsRequired();
    }
}
