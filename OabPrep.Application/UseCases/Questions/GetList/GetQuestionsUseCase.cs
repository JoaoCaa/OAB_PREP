using OabPrep.Application.Common.Interfaces;
using OabPrep.Application.Common.Models;
using OabPrep.Domain.Enums;

namespace OabPrep.Application.UseCases.Questions.GetList;

public sealed class GetQuestionsUseCase
{
    private readonly IQuestionRepository _repository;

    public GetQuestionsUseCase(IQuestionRepository repository) => _repository = repository;

    public async Task<PagedResult<QuestionSummaryResponse>> ExecuteAsync(
        GetQuestionsQuery query,
        CancellationToken ct = default)
    {
        var filter = new QuestionFilter(
            query.AreaId,
            query.Year,
            query.Difficulty.HasValue ? (DifficultyLevel)query.Difficulty.Value : null,
            query.Search);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var (items, totalCount) = await _repository.GetPagedAsync(filter, page, pageSize, ct);

        var summaries = items
            .Select(q => new QuestionSummaryResponse(
                q.Id,
                q.LawAreaId,
                q.LawArea?.Name ?? string.Empty,
                q.Statement,
                q.Year,
                q.ExamEdition,
                (int)q.Difficulty,
                q.IsActive))
            .ToList();

        return new PagedResult<QuestionSummaryResponse>(summaries, totalCount, page, pageSize);
    }
}
