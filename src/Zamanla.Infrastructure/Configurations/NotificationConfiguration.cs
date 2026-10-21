using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zamanla.Domain.Appointments;
using Zamanla.Domain.Notifications;
using Zamanla.Domain.Tenants;

namespace Zamanla.Infrastructure.Configurations;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);

        builder.OwnsOne(n => n.Type, type =>
        {
            type.Property(t => t.Value).HasColumnName("Type").HasMaxLength(30);
        });

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(n => n.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Appointment>()
            .WithMany()
            .HasForeignKey(n => n.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(n => n.Type).IsRequired();
    }
}
