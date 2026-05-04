using OabPrep.Domain.Common;
using OabPrep.Domain.Enums;

namespace OabPrep.Domain.Entities;

public sealed class Session : BaseEntity<int>
{
    private Session() { }

    private readonly List<SessionAnswer> _answers = [];

    public Guid UserId { get; private set; }
    public SessionStatus Status { get; private set; } = SessionStatus.InProgress;

    public IReadOnlyList<SessionAnswer> Answers => _answers.AsReadOnly();

    public static Session Create(Guid userId, IEnumerable<int> questionIds)
    {
        var session = new Session
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var qId in questionIds)
            session._answers.Add(SessionAnswer.CreatePending(qId));

        return session;
    }

    public void Complete()
    {
        Status = SessionStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Abandon()
    {
        Status = SessionStatus.Abandoned;
        UpdatedAt = DateTime.UtcNow;
    }
}
