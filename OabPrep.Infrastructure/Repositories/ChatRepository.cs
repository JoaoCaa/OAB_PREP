using Microsoft.EntityFrameworkCore;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;
using OabPrep.Infrastructure.Persistence;

namespace OabPrep.Infrastructure.Repositories;

public sealed class ChatRepository : IChatRepository
{
    private readonly ApplicationDbContext _context;

    public ChatRepository(ApplicationDbContext context) => _context = context;

    public async Task<IList<ChatMessage>> GetHistoryAsync(
        Guid userId, int? sessionId, int? questionId, int limit, CancellationToken ct = default)
    {
        var query = _context.ChatMessages
            .Where(m => m.UserId == userId);

        if (sessionId.HasValue)
            query = query.Where(m => m.SessionId == sessionId);
        if (questionId.HasValue)
            query = query.Where(m => m.QuestionId == questionId);

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(Guid userId, int sessionId, int questionId, CancellationToken ct = default) =>
        _context.ChatMessages
            .CountAsync(m => m.UserId == userId
                          && m.SessionId == sessionId
                          && m.QuestionId == questionId, ct);

    public async Task<QuestionContext?> GetQuestionContextAsync(
        int questionId, int? sessionId, CancellationToken ct = default)
    {
        var question = await _context.Questions
            .Include(q => q.LawArea)
            .Include(q => q.Alternatives)
            .FirstOrDefaultAsync(q => q.Id == questionId, ct);

        if (question is null) return null;

        var isAnswered = false;
        if (sessionId.HasValue)
        {
            isAnswered = await _context.SessionAnswers
                .AnyAsync(a => a.SessionId == sessionId.Value
                            && a.QuestionId == questionId
                            && a.AnsweredAt.HasValue, ct);
        }

        var alternatives = question.Alternatives
            .OrderBy(a => a.Letter)
            .Select(a => new AlternativeInfo(a.Letter, a.Text))
            .ToList();

        var correctLetter = question.Alternatives.First(a => a.IsCorrect).Letter;

        return new QuestionContext(
            question.Statement,
            question.LawArea?.Name ?? "Geral",
            question.LegalRefs,
            isAnswered,
            alternatives,
            correctLetter);
    }

    public async Task AddAsync(ChatMessage message, CancellationToken ct = default)
    {
        await _context.ChatMessages.AddAsync(message, ct);
    }
}
