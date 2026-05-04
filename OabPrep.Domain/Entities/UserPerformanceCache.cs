using OabPrep.Domain.Common;

namespace OabPrep.Domain.Entities;

public sealed class UserPerformanceCache : BaseEntity<int>
{
    private UserPerformanceCache() { }

    public Guid UserId { get; private set; }
    public int? LawAreaId { get; private set; }
    public string LawAreaName { get; private set; } = string.Empty;
    public int TotalAnswered { get; private set; }
    public int CorrectAnswers { get; private set; }
    public decimal AccuracyPct { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }

    public static UserPerformanceCache Create(
        Guid userId, int? lawAreaId, string lawAreaName, int total, int correct)
    {
        return new UserPerformanceCache
        {
            UserId = userId,
            LawAreaId = lawAreaId,
            LawAreaName = lawAreaName,
            TotalAnswered = total,
            CorrectAnswers = correct,
            AccuracyPct = ComputePct(total, correct),
            LastUpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(int total, int correct)
    {
        TotalAnswered = total;
        CorrectAnswers = correct;
        AccuracyPct = ComputePct(total, correct);
        LastUpdatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private static decimal ComputePct(int total, int correct) =>
        total > 0 ? Math.Round((decimal)correct / total * 100, 2) : 0m;
}
