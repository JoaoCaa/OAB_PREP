using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OabPrep.Domain.Entities;

namespace OabPrep.Infrastructure.Persistence.Configurations;

public sealed class EmailTokenConfiguration : IEntityTypeConfiguration<EmailToken>
{
    public void Configure(EntityTypeBuilder<EmailToken> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(t => t.Token)
            .IsUnique();

        builder.Property(t => t.TokenType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.ExpiresAt)
            .IsRequired();

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
