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
