using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OabPrep.Domain.Entities;

namespace OabPrep.Infrastructure.Persistence.Configurations;

public sealed class AlternativeConfiguration : IEntityTypeConfiguration<Alternative>
{
    public void Configure(EntityTypeBuilder<Alternative> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd();

        builder.Property(a => a.Letter)
            .IsRequired()
            .HasMaxLength(1);

        builder.Property(a => a.Text)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.IsCorrect)
            .IsRequired();

        builder.Property(a => a.Explanation)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.HasIndex(a => new { a.QuestionId, a.Letter })
            .IsUnique();
    }
}
