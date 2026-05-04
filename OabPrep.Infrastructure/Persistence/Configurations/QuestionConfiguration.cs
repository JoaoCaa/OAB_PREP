using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OabPrep.Domain.Entities;

namespace OabPrep.Infrastructure.Persistence.Configurations;

public sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Id)
            .ValueGeneratedOnAdd();

        builder.Property(q => q.Statement)
            .IsRequired()
            .HasMaxLength(3000);

        builder.Property(q => q.Year)
            .IsRequired();

        builder.Property(q => q.ExamEdition)
            .HasMaxLength(50);

        builder.Property(q => q.Explanation)
            .HasMaxLength(5000);

        builder.Property(q => q.LegalRefs)
            .HasMaxLength(2000);

        builder.Property(q => q.Difficulty)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(q => q.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(q => q.CreatedAt)
            .IsRequired();

        builder.HasOne(q => q.LawArea)
            .WithMany()
            .HasForeignKey(q => q.LawAreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(q => q.Alternatives)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(q => q.Alternatives)
            .HasField("_alternatives")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
