using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OabPrep.Domain.Entities;

namespace OabPrep.Infrastructure.Persistence.Configurations;

public sealed class UserPerformanceCacheConfiguration : IEntityTypeConfiguration<UserPerformanceCache>
{
    public void Configure(EntityTypeBuilder<UserPerformanceCache> builder)
    {
        builder.ToTable("UserPerformanceCache");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId).IsRequired();
        builder.Property(c => c.LawAreaId);
        builder.Property(c => c.LawAreaName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.TotalAnswered).IsRequired();
        builder.Property(c => c.CorrectAnswers).IsRequired();
        builder.Property(c => c.AccuracyPct).IsRequired().HasPrecision(5, 2);
        builder.Property(c => c.LastUpdatedAt).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt);

        builder.HasIndex(c => new { c.UserId, c.LawAreaId }).IsUnique();
    }
}
