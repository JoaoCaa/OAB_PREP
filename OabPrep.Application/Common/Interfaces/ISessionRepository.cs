using OabPrep.Domain.Entities;

namespace OabPrep.Application.Common.Interfaces;

public interface ISessionRepository
{
    Task AddAsync(Session session, CancellationToken cancellationToken = default);
    Task<Session?> FindByIdWithAnswersAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<int>> GetCorrectlyAnsweredQuestionIdsAsync(Guid userId, CancellationToken cancellationToken = default);
}
