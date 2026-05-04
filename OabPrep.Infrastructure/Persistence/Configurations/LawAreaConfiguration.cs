using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OabPrep.Domain.Entities;

namespace OabPrep.Infrastructure.Persistence.Configurations;

public sealed class LawAreaConfiguration : IEntityTypeConfiguration<LawArea>
{
    public void Configure(EntityTypeBuilder<LawArea> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .ValueGeneratedOnAdd();

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(l => l.Slug)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(l => l.Slug)
            .IsUnique();

        builder.Property(l => l.Description)
            .HasMaxLength(500);

        builder.Property(l => l.IconUrl)
            .HasMaxLength(300);

        builder.Property(l => l.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(l => l.CreatedAt)
            .IsRequired();
    }
}
