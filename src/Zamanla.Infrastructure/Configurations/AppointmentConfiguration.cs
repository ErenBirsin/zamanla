using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Customers;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Tenants;

namespace Zamanla.Infrastructure.Configurations;

internal sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        builder.HasKey(a => a.Id);

        builder.OwnsOne(a => a.AppointmentNumber, number =>
        {
            number.Property(n => n.Value).HasColumnName("AppointmentNumber").HasMaxLength(20);
            number.HasIndex(n => n.Value).IsUnique();
        });

        builder.OwnsOne(a => a.Time, time =>
        {
            time.Property(t => t.Start).HasColumnName("StartAt");
            time.Property(t => t.End).HasColumnName("EndAt");
            time.HasIndex(t => t.Start);
        });

        builder.OwnsOne(a => a.Total, total =>
        {
            total.Property(t => t.Value).HasColumnName("Total").HasPrecision(18, 2);
        });

        builder.OwnsOne(a => a.Status, status =>
        {
            status.Property(s => s.Value).HasColumnName("Status").HasMaxLength(20);
        });

        builder.OwnsOne(a => a.Source, source =>
        {
            source.Property(s => s.Value).HasColumnName("Source").HasMaxLength(20);
        });

        builder.OwnsOne(a => a.Note, note =>
        {
            note.Property(n => n.Value).HasColumnName("Note").HasMaxLength(500);
        });

        builder.OwnsOne(a => a.ManageToken, manageToken =>
        {
            manageToken.Property(t => t.Value).HasColumnName("ManageToken").HasMaxLength(64);
            // linkten gelen token'ın özetiyle randevu bulunur
            manageToken.HasIndex(t => t.Value).IsUnique();
        });

        builder.OwnsMany(a => a.SalonServices, salonService =>
        {
            salonService.ToTable("AppointmentSalonServices");
            salonService.Property(s => s.Price).HasPrecision(18, 2);
        });

        builder.OwnsMany(a => a.Histories, history =>
        {
            history.ToTable("AppointmentHistories");
            history.Property(h => h.Title).HasMaxLength(100);
            history.Property(h => h.Description).HasMaxLength(500);
        });

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Staff>()
            .WithMany()
            .HasForeignKey(a => a.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(a => a.AppointmentNumber).IsRequired();
        builder.Navigation(a => a.Time).IsRequired();
        builder.Navigation(a => a.Total).IsRequired();
        builder.Navigation(a => a.Status).IsRequired();
        builder.Navigation(a => a.Source).IsRequired();
    }
}
