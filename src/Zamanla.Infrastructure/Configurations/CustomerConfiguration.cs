using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zamanla.Domain.Customers;
using Zamanla.Domain.Tenants;

namespace Zamanla.Infrastructure.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);

        builder.OwnsOne(c => c.FirstName, firstName =>
        {
            firstName.Property(f => f.Value).HasColumnName("FirstName").HasMaxLength(100);
        });

        builder.OwnsOne(c => c.LastName, lastName =>
        {
            lastName.Property(l => l.Value).HasColumnName("LastName").HasMaxLength(100);
        });

        builder.OwnsOne(c => c.PhoneNumber, phoneNumber =>
        {
            phoneNumber.Property(p => p.Value).HasColumnName("PhoneNumber").HasMaxLength(13);
            // telefonla arama ve randevu sayfasında müşteriyi bulmak için
            phoneNumber.HasIndex(p => p.Value);
        });

        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("Email").HasMaxLength(254);
        });

        builder.OwnsOne(c => c.Note, note =>
        {
            note.Property(n => n.Value).HasColumnName("Note").HasMaxLength(500);
        });

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(c => c.FirstName).IsRequired();
        builder.Navigation(c => c.LastName).IsRequired();
    }
}
