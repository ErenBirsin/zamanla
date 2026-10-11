using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zamanla.Domain.RefreshTokens;
using Zamanla.Domain.Users;

namespace Zamanla.Infrastructure.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Token, token =>
        {
            token.Property(v => v.Value).HasColumnName("Token").HasMaxLength(64);
            token.HasIndex(v => v.Value).IsUnique();
        });

        builder.OwnsOne(x => x.IsActive, isActive =>
        {
            isActive.Property(v => v.Value).HasColumnName("IsActive");
        });

        builder.OwnsOne(x => x.ExpiresDate, expiresDate =>
        {
            expiresDate.Property(v => v.Value).HasColumnName("ExpiresDate");
        });

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Token).IsRequired();
        builder.Navigation(x => x.IsActive).IsRequired();
        builder.Navigation(x => x.ExpiresDate).IsRequired();
    }
}
