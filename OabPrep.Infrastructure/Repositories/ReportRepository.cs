using Microsoft.EntityFrameworkCore;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Infrastructure.Persistence;

namespace OabPrep.Infrastructure.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private readonly ApplicationDbContext _context;

    public ReportRepository(ApplicationDbContext context) => _context = context;

    public async Task<SystemSummaryData> GetSystemSummaryDataAsync(CancellationToken cancellationToken = default)
    {
        var cutoff30d = DateTime.UtcNow.AddDays(-30);

        var totalUsersTask        = _context.Users.CountAsync(cancellationToken);
        var activeUsersTask       = _context.Sessions
                                        .Where(s => s.CreatedAt >= cutoff30d)
                                        .Select(s => s.UserId)
                                        .Distinct()
                                        .CountAsync(cancellationToken);
        var totalQuestionsTask    = _context.Questions.CountAsync(q => q.IsActive, cancellationToken);
        var totalSessionsTask     = _context.Sessions.CountAsync(cancellationToken);
        var avgAccuracyTask       = _context.UserPerformanceCaches
                                        .Where(c => !c.LawAreaId.HasValue && c.TotalAnswered > 0)
                                        .AverageAsync(c => (decimal?)c.AccuracyPct, cancellationToken);

        await Task.WhenAll(totalUsersTask, activeUsersTask, totalQuestionsTask,
                           totalSessionsTask, avgAccuracyTask);

        // Top weak areas: areas with highest error rate from SessionAnswers
        var rawAreas = await _context.SessionAnswers
            .Where(a => a.IsCorrect.HasValue && a.Question!.LawArea != null)
            .GroupBy(a => new { a.Question!.LawAreaId, Name = a.Question.LawArea!.Name })
            .Select(g => new
            {
                g.Key.Name,
                Total   = g.Count(),
                Errors  = g.Count(a => a.IsCorrect == false)
            })
            .ToListAsync(cancellationToken);

        var topWeakAreas = rawAreas
            .Where(a => a.Total > 0)
            .Select(a => new WeakAreaData(a.Name, Math.Round((decimal)a.Errors / a.Total * 100, 2)))
            .OrderByDescending(a => a.ErrorRatePct)
            .Take(5)
            .ToList();

        // Registrations by month
        var rawRegs = await _context.Users
            .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(r => r.Year).ThenBy(r => r.Month)
            .ToListAsync(cancellationToken);

        var regsByMonth = rawRegs
            .Select(r => new RegistrationsByMonthData(r.Year, r.Month, r.Count))
            .ToList();

        return new SystemSummaryData(
            totalUsersTask.Result,
            activeUsersTask.Result,
            totalQuestionsTask.Result,
            totalSessionsTask.Result,
            Math.Round(avgAccuracyTask.Result ?? 0m, 2),
            topWeakAreas,
            regsByMonth);
    }

    public async Task<IList<QuestionStatData>> GetQuestionStatsAsync(
        int? areaId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SessionAnswers
            .Where(a => a.IsCorrect.HasValue && a.Question != null);

        if (areaId.HasValue)
            query = query.Where(a => a.Question!.LawAreaId == areaId.Value);

        var raw = await query
            .GroupBy(a => new
            {
                a.QuestionId,
                Statement   = a.Question!.Statement,
                LawAreaName = a.Question.LawArea!.Name
            })
            .Select(g => new
            {
                g.Key.QuestionId,
                g.Key.Statement,
                g.Key.LawAreaName,
                Total      = g.Count(),
                Errors     = g.Count(a => a.IsCorrect == false),
                AvgTimeSec = g.Average(a => (double?)a.TimeSpentSeconds)
            })
            .OrderByDescending(x => x.Errors)
            .Take(20)
            .ToListAsync(cancellationToken);

        return raw.Select(r =>
        {
            var errorRate = r.Total > 0 ? Math.Round((decimal)r.Errors / r.Total * 100, 2) : 0m;
            var avgTime   = r.AvgTimeSec.HasValue ? Math.Round((decimal)r.AvgTimeSec.Value, 2) : 0m;
            return new QuestionStatData(
                r.QuestionId, r.Statement, r.LawAreaName, r.Errors, r.Total, errorRate, avgTime);
        }).ToList();
    }
}
