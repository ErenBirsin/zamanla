using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zamanla.Domain.Invitations;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Tenants;

namespace Zamanla.Infrastructure.Configurations;

internal sealed class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("Invitations");
        builder.HasKey(i => i.Id);

        builder.OwnsOne(i => i.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("Email").HasMaxLength(254);
        });

        builder.OwnsOne(i => i.Token, token =>
        {
            token.Property(t => t.Value).HasColumnName("Token").HasMaxLength(64);
            token.HasIndex(t => t.Value).IsUnique();
        });

        builder.OwnsMany(i => i.Permissions, permission =>
        {
            permission.ToTable("InvitationPermissions");
            permission.Property(p => p.Value).HasColumnName("Value").HasMaxLength(50);
        });

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(i => i.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Staff>()
            .WithMany()
            .HasForeignKey(i => i.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(i => i.Email).IsRequired();
        builder.Navigation(i => i.Token).IsRequired();
    }
}
