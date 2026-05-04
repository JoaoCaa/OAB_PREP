using OabPrep.Domain.Common;

namespace OabPrep.Domain.Entities;

public sealed class SessionAnswer : BaseEntity<int>
{
    private SessionAnswer() { }

    public int SessionId { get; private set; }
    public int QuestionId { get; private set; }
    public int? ChosenAlternativeId { get; private set; }
    public bool? IsCorrect { get; private set; }
    public int? TimeSpentSeconds { get; private set; }
    public DateTime? AnsweredAt { get; private set; }

    public Session? Session { get; private set; }
    public Question? Question { get; private set; }

    internal static SessionAnswer CreatePending(int questionId) =>
        new() { QuestionId = questionId, CreatedAt = DateTime.UtcNow };

    public void Answer(int chosenAlternativeId, bool isCorrect, int? timeSpentSeconds)
    {
        ChosenAlternativeId = chosenAlternativeId;
        IsCorrect = isCorrect;
        TimeSpentSeconds = timeSpentSeconds;
        AnsweredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
