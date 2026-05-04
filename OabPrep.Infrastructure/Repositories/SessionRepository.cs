using Microsoft.EntityFrameworkCore;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;
using OabPrep.Infrastructure.Persistence;

namespace OabPrep.Infrastructure.Repositories;

public sealed class SessionRepository : ISessionRepository
{
    private readonly ApplicationDbContext _context;

    public SessionRepository(ApplicationDbContext context) => _context = context;

    public async Task AddAsync(Session session, CancellationToken cancellationToken = default) =>
        await _context.Sessions.AddAsync(session, cancellationToken);

    public Task<Session?> FindByIdWithAnswersAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Sessions
            .Include(s => s.Answers)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Session?> FindByIdForFinishAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Sessions
            .Include(s => s.Answers)
                .ThenInclude(a => a.Question)
                    .ThenInclude(q => q!.LawArea)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Session?> FindByIdFullAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Sessions
            .Include(s => s.Answers)
                .ThenInclude(a => a.Question)
                    .ThenInclude(q => q!.LawArea)
            .Include(s => s.Answers)
                .ThenInclude(a => a.Question)
                    .ThenInclude(q => q!.Alternatives)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IList<AreaAnswerStats>> GetAreaStatsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var raw = await _context.SessionAnswers
            .Where(a => a.Session!.UserId == userId && a.IsCorrect.HasValue)
            .GroupBy(a => new { a.Question!.LawAreaId, LawAreaName = a.Question.LawArea!.Name })
            .Select(g => new
            {
                g.Key.LawAreaId,
                g.Key.LawAreaName,
                Total = g.Count(),
                Correct = g.Count(a => a.IsCorrect == true)
            })
            .ToListAsync(cancellationToken);

        return raw.Select(r => new AreaAnswerStats(r.LawAreaId, r.LawAreaName, r.Total, r.Correct))
                  .ToList();
    }

    public Task<SessionAnswer?> FindSessionAnswerAsync(
        int sessionId,
        int questionId,
        CancellationToken cancellationToken = default) =>
        _context.SessionAnswers
            .Include(a => a.Session)
            .FirstOrDefaultAsync(a => a.SessionId == sessionId && a.QuestionId == questionId, cancellationToken);

    public async Task<(int TotalSessions, decimal AvgTimePerQuestion)> GetSummaryStatsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var totalSessions = await _context.Sessions
            .CountAsync(s => s.UserId == userId, cancellationToken);

        var avgTime = await _context.SessionAnswers
            .Where(a => a.Session!.UserId == userId && a.TimeSpentSeconds.HasValue)
            .AverageAsync(a => (double?)a.TimeSpentSeconds, cancellationToken);

        return (totalSessions, avgTime.HasValue ? Math.Round((decimal)avgTime.Value, 2) : 0m);
    }

    public async Task<IList<DailyTrendPoint>> GetTrendAsync(
        Guid userId,
        DateTime? since,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SessionAnswers
            .Where(a => a.Session!.UserId == userId && a.IsCorrect.HasValue && a.AnsweredAt.HasValue);

        if (since.HasValue)
            query = query.Where(a => a.AnsweredAt >= since.Value);

        var raw = await query
            .GroupBy(a => a.AnsweredAt!.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                Total = g.Count(),
                Correct = g.Count(a => a.IsCorrect == true)
            })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return raw.Select(r => new DailyTrendPoint(DateOnly.FromDateTime(r.Date), r.Total, r.Correct))
                  .ToList();
    }

    public async Task<IList<WrongQuestionData>> GetRecentWrongQuestionsAsync(
        Guid userId,
        int lawAreaId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var raw = await _context.SessionAnswers
            .Where(a => a.Session!.UserId == userId
                     && a.IsCorrect == false
                     && a.Question!.LawAreaId == lawAreaId
                     && a.AnsweredAt.HasValue)
            .OrderByDescending(a => a.AnsweredAt)
            .Take(limit)
            .Select(a => new { a.QuestionId, Statement = a.Question!.Statement, a.AnsweredAt })
            .ToListAsync(cancellationToken);

        return raw.Select(r => new WrongQuestionData(r.QuestionId, r.Statement, r.AnsweredAt!.Value))
                  .ToList();
    }

    public async Task<IList<DailyTrendPoint>> GetAreaEvolutionAsync(
        Guid userId,
        int lawAreaId,
        CancellationToken cancellationToken = default)
    {
        var raw = await _context.SessionAnswers
            .Where(a => a.Session!.UserId == userId
                     && a.IsCorrect.HasValue
                     && a.AnsweredAt.HasValue
                     && a.Question!.LawAreaId == lawAreaId)
            .GroupBy(a => a.AnsweredAt!.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                Total = g.Count(),
                Correct = g.Count(a => a.IsCorrect == true)
            })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return raw.Select(r => new DailyTrendPoint(DateOnly.FromDateTime(r.Date), r.Total, r.Correct))
                  .ToList();
    }

    public async Task<Dictionary<Guid, int>> GetSessionCountsByUserIdsAsync(
        IList<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .Where(s => userIds.Contains(s.UserId))
            .GroupBy(s => s.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellationToken);
    }

    public async Task<IReadOnlyCollection<int>> GetCorrectlyAnsweredQuestionIdsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var ids = await _context.SessionAnswers
            .Where(a => a.Session!.UserId == userId && a.IsCorrect == true)
            .Select(a => a.QuestionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return ids;
    }
}
