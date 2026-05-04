using OabPrep.Domain.Entities;

namespace OabPrep.Application.Common.Interfaces;

public record AreaAnswerStats(int LawAreaId, string LawAreaName, int Total, int Correct);

public interface ISessionRepository
{
    Task AddAsync(Session session, CancellationToken cancellationToken = default);
    Task<Session?> FindByIdWithAnswersAsync(int id, CancellationToken cancellationToken = default);
    Task<Session?> FindByIdForFinishAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<int>> GetCorrectlyAnsweredQuestionIdsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IList<AreaAnswerStats>> GetAreaStatsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<SessionAnswer?> FindSessionAnswerAsync(int sessionId, int questionId, CancellationToken cancellationToken = default);
    Task<Session?> FindByIdFullAsync(int id, CancellationToken cancellationToken = default);
}
