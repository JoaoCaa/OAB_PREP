using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OabPrep.Domain.Entities;

namespace OabPrep.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Token)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(e => e.Token).IsUnique();

        builder.Property(e => e.ExpiresAt).IsRequired();
        builder.Property(e => e.UsedAt).IsRequired(false);
        builder.Property(e => e.RevokedAt).IsRequired(false);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
