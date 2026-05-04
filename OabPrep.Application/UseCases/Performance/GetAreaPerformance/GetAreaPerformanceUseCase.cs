using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Application.UseCases.Performance.GetAreaPerformance;

public sealed class GetAreaPerformanceUseCase
{
    private readonly ILawAreaRepository _lawAreaRepository;
    private readonly IUserPerformanceCacheRepository _cacheRepository;
    private readonly ISessionRepository _sessionRepository;

    public GetAreaPerformanceUseCase(
        ILawAreaRepository lawAreaRepository,
        IUserPerformanceCacheRepository cacheRepository,
        ISessionRepository sessionRepository)
    {
        _lawAreaRepository = lawAreaRepository;
        _cacheRepository = cacheRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<GetAreaPerformanceResponse> ExecuteAsync(
        Guid userId,
        int areaId,
        CancellationToken ct = default)
    {
        var area = await _lawAreaRepository.FindByIdAsync(areaId, ct)
            ?? throw new NotFoundException("Área do direito", areaId);

        var cacheEntries = await _cacheRepository.GetByUserIdAsync(userId, ct);
        var areaCache = cacheEntries.FirstOrDefault(c => c.LawAreaId == areaId);

        var wrongQuestionsTask = _sessionRepository.GetRecentWrongQuestionsAsync(userId, areaId, 10, ct);
        var evolutionTask = _sessionRepository.GetAreaEvolutionAsync(userId, areaId, ct);

        await Task.WhenAll(wrongQuestionsTask, evolutionTask);

        var recentWrong = wrongQuestionsTask.Result
            .Select(w => new WrongQuestionResponse(
                w.QuestionId,
                w.Statement.Length > 120 ? w.Statement[..120] + "…" : w.Statement,
                w.AnsweredAt))
            .ToList();

        var evolution = evolutionTask.Result
            .Select(t =>
            {
                var pct = t.Total > 0 ? Math.Round((decimal)t.Correct / t.Total * 100, 2) : 0m;
                return new AreaEvolutionPointResponse(t.Date, pct, t.Total);
            })
            .ToList();

        return new GetAreaPerformanceResponse(
            area.Name,
            areaCache?.TotalAnswered ?? 0,
            areaCache?.CorrectAnswers ?? 0,
            areaCache?.AccuracyPct ?? 0m,
            recentWrong,
            evolution);
    }
}
