using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Tenants;
using Zamanla.Domain.TimeOffs;

namespace Zamanla.Infrastructure.Configurations;

internal sealed class TimeOffConfiguration : IEntityTypeConfiguration<TimeOff>
{
    public void Configure(EntityTypeBuilder<TimeOff> builder)
    {
        builder.ToTable("TimeOffs");
        builder.HasKey(t => t.Id);

        builder.OwnsOne(t => t.Time, time =>
        {
            time.Property(r => r.Start).HasColumnName("StartAt");
            time.Property(r => r.End).HasColumnName("EndAt");
            // takvim ve boş saat hesabı tarih aralığıyla arar
            time.HasIndex(r => r.Start);
        });

        builder.OwnsOne(t => t.Note, note =>
        {
            note.Property(n => n.Value).HasColumnName("Note").HasMaxLength(250);
        });

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(t => t.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Staff>()
            .WithMany()
            .HasForeignKey(t => t.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(t => t.Time).IsRequired();
    }
}
