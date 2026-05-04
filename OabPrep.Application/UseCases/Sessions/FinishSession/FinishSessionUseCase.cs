using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;
using OabPrep.Domain.Enums;

namespace OabPrep.Application.UseCases.Sessions.FinishSession;

public sealed class FinishSessionUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IUserPerformanceCacheRepository _cacheRepository;
    private readonly IApplicationDbContext _context;

    public FinishSessionUseCase(
        ISessionRepository sessionRepository,
        IUserPerformanceCacheRepository cacheRepository,
        IApplicationDbContext context)
    {
        _sessionRepository = sessionRepository;
        _cacheRepository = cacheRepository;
        _context = context;
    }

    public async Task<FinishSessionResponse> ExecuteAsync(
        int sessionId,
        Guid userId,
        CancellationToken ct = default)
    {
        var session = await _sessionRepository.FindByIdForFinishAsync(sessionId, ct)
            ?? throw new NotFoundException("Sessão", sessionId);

        if (session.UserId != userId)
            throw new ForbiddenException();

        if (session.Status != SessionStatus.InProgress)
            throw new ConflictException("A sessão já foi finalizada.");

        session.Complete();

        // --- build current-session stats ---
        var answers = session.Answers;
        var totalQuestions = answers.Count;
        var correctAnswers = answers.Count(a => a.IsCorrect == true);
        var timeSpent = answers.Sum(a => a.TimeSpentSeconds ?? 0);
        var sessionAccuracy = totalQuestions > 0
            ? Math.Round((decimal)correctAnswers / totalQuestions * 100, 2)
            : 0m;

        var byArea = answers
            .Where(a => a.Question?.LawArea is not null)
            .GroupBy(a => new { a.Question!.LawAreaId, a.Question.LawArea!.Name })
            .Select(g =>
            {
                var total = g.Count();
                var correct = g.Count(a => a.IsCorrect == true);
                var pct = total > 0 ? Math.Round((decimal)correct / total * 100, 2) : 0m;
                return new AreaPerformanceResponse(g.Key.LawAreaId, g.Key.Name, total, correct, pct);
            })
            .ToList();

        var weakAreas = byArea.Where(a => a.AccuracyPct < 50m).Select(a => a.LawAreaName).ToList();

        // --- recalculate UserPerformanceCache (full history) ---
        var allStats = await _sessionRepository.GetAreaStatsForUserAsync(userId, ct);
        await UpsertPerformanceCacheAsync(userId, allStats, ct);

        await _context.SaveChangesAsync(ct);

        return new FinishSessionResponse(
            session.Id,
            totalQuestions,
            correctAnswers,
            sessionAccuracy,
            timeSpent,
            byArea,
            weakAreas);
    }

    private async Task UpsertPerformanceCacheAsync(
        Guid userId,
        IList<AreaAnswerStats> stats,
        CancellationToken ct)
    {
        var existing = await _cacheRepository.GetByUserIdAsync(userId, ct);
        var cacheByArea = existing
            .Where(c => c.LawAreaId.HasValue)
            .ToDictionary(c => c.LawAreaId!.Value);
        var globalEntry = existing.FirstOrDefault(c => !c.LawAreaId.HasValue);

        foreach (var stat in stats)
        {
            if (cacheByArea.TryGetValue(stat.LawAreaId, out var entry))
                entry.Update(stat.Total, stat.Correct);
            else
                await _cacheRepository.AddAsync(
                    UserPerformanceCache.Create(userId, stat.LawAreaId, stat.LawAreaName, stat.Total, stat.Correct), ct);
        }

        var globalTotal = stats.Sum(s => s.Total);
        var globalCorrect = stats.Sum(s => s.Correct);
        if (globalEntry is not null)
            globalEntry.Update(globalTotal, globalCorrect);
        else
            await _cacheRepository.AddAsync(
                UserPerformanceCache.Create(userId, null, "Global", globalTotal, globalCorrect), ct);
    }
}
