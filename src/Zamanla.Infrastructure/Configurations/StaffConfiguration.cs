using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Tenants;

namespace Zamanla.Infrastructure.Configurations;

internal sealed class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("Staffs");
        builder.HasKey(s => s.Id);


        builder.Property(s => s.BookingVersion).IsConcurrencyToken();

        builder.OwnsOne(s => s.FirstName, firstName =>
        {
            firstName.Property(f => f.Value).HasColumnName("FirstName").HasMaxLength(100);
        });

        builder.OwnsOne(s => s.LastName, lastName =>
        {
            lastName.Property(l => l.Value).HasColumnName("LastName").HasMaxLength(100);
        });

        builder.OwnsOne(s => s.Color, color =>
        {
            color.Property(c => c.Value).HasColumnName("Color").HasMaxLength(20);
        });

        builder.OwnsMany(s => s.WorkingHours, workingHour =>
        {
            workingHour.ToTable("StaffWorkingHours");
        });

        builder.OwnsMany(s => s.SalonServices, salonService =>
        {
            salonService.ToTable("StaffSalonServices");
        });

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(s => s.FirstName).IsRequired();
        builder.Navigation(s => s.LastName).IsRequired();
        builder.Navigation(s => s.Color).IsRequired();
    }
}
