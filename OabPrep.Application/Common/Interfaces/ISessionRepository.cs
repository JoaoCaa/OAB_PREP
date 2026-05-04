using OabPrep.Domain.Entities;

namespace OabPrep.Application.Common.Interfaces;

public record AreaAnswerStats(int LawAreaId, string LawAreaName, int Total, int Correct);
public record DailyTrendPoint(DateOnly Date, int Total, int Correct);
public record WrongQuestionData(int QuestionId, string Statement, DateTime AnsweredAt);

public interface ISessionRepository
{
    Task AddAsync(Session session, CancellationToken cancellationToken = default);
    Task<Session?> FindByIdWithAnswersAsync(int id, CancellationToken cancellationToken = default);
    Task<Session?> FindByIdForFinishAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<int>> GetCorrectlyAnsweredQuestionIdsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IList<AreaAnswerStats>> GetAreaStatsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<SessionAnswer?> FindSessionAnswerAsync(int sessionId, int questionId, CancellationToken cancellationToken = default);
    Task<Session?> FindByIdFullAsync(int id, CancellationToken cancellationToken = default);
    Task<(int TotalSessions, decimal AvgTimePerQuestion)> GetSummaryStatsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IList<DailyTrendPoint>> GetTrendAsync(Guid userId, DateTime? since, CancellationToken cancellationToken = default);
    Task<IList<WrongQuestionData>> GetRecentWrongQuestionsAsync(Guid userId, int lawAreaId, int limit, CancellationToken cancellationToken = default);
    Task<IList<DailyTrendPoint>> GetAreaEvolutionAsync(Guid userId, int lawAreaId, CancellationToken cancellationToken = default);
}
