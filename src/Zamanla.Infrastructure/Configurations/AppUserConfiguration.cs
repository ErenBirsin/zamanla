using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zamanla.Domain.Staffs;
using Zamanla.Domain.Tenants;
using Zamanla.Domain.Users;

namespace Zamanla.Infrastructure.Configurations;

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("AppUsers");
        builder.HasKey(u => u.Id);

        builder.OwnsOne(u => u.FirstName, firstName =>
        {
            firstName.Property(f => f.Value).HasColumnName("FirstName").HasMaxLength(100);
        });

        builder.OwnsOne(u => u.LastName, lastName =>
        {
            lastName.Property(l => l.Value).HasColumnName("LastName").HasMaxLength(100);
        });

        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("Email").HasMaxLength(254);
            email.HasIndex(e => e.Value).IsUnique();
        });

        builder.OwnsOne(u => u.Password, password =>
        {
            password.Property(p => p.PasswordHash).HasColumnName("PasswordHash").HasMaxLength(256);
        });

        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

        builder.OwnsMany(u => u.Permissions, permission =>
        {
            permission.ToTable("AppUserPermissions");
            permission.Property(p => p.Value).HasColumnName("Value").HasMaxLength(50);
        });

        // bir personelin en fazla bir hesabı olur
        builder.HasIndex(u => u.StaffId).IsUnique();
        builder.HasOne<Staff>()
            .WithMany()
            .HasForeignKey(u => u.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(u => u.FirstName).IsRequired();
        builder.Navigation(u => u.LastName).IsRequired();
        builder.Navigation(u => u.Email).IsRequired();
        builder.Navigation(u => u.Password).IsRequired();
    }
}
