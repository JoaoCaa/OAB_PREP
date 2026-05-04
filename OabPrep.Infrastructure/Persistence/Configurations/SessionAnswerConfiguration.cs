using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OabPrep.Domain.Entities;

namespace OabPrep.Infrastructure.Persistence.Configurations;

public sealed class SessionAnswerConfiguration : IEntityTypeConfiguration<SessionAnswer>
{
    public void Configure(EntityTypeBuilder<SessionAnswer> builder)
    {
        builder.ToTable("SessionAnswers");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.SessionId).IsRequired();
        builder.Property(a => a.QuestionId).IsRequired();
        builder.Property(a => a.ChosenAlternativeId);
        builder.Property(a => a.IsCorrect);
        builder.Property(a => a.TimeSpentSeconds);
        builder.Property(a => a.AnsweredAt);
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt);

        builder.HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
