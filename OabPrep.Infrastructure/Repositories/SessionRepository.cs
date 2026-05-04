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
