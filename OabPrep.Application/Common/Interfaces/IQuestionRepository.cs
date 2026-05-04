using OabPrep.Domain.Entities;
using OabPrep.Domain.Enums;

namespace OabPrep.Application.Common.Interfaces;

public record QuestionFilter(int? AreaId, int? Year, DifficultyLevel? Difficulty, string? Search);

public interface IQuestionRepository
{
    Task<(IList<Question> Items, int TotalCount)> GetPagedAsync(
        QuestionFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Question?> FindByIdWithAlternativesAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Question question, CancellationToken cancellationToken = default);
}
