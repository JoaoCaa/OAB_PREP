using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Application.UseCases.Sessions.ToggleReviewMark;

public sealed class ToggleReviewMarkUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IApplicationDbContext _context;

    public ToggleReviewMarkUseCase(ISessionRepository sessionRepository, IApplicationDbContext context)
    {
        _sessionRepository = sessionRepository;
        _context = context;
    }

    public async Task<ToggleReviewMarkResponse> ExecuteAsync(
        int sessionId,
        int questionId,
        ToggleReviewMarkCommand command,
        Guid userId,
        CancellationToken ct = default)
    {
        var answer = await _sessionRepository.FindSessionAnswerAsync(sessionId, questionId, ct)
            ?? throw new NotFoundException("Questão na sessão", questionId);

        if (answer.Session!.UserId != userId)
            throw new ForbiddenException();

        answer.MarkForReview(command.Marked);

        await _context.SaveChangesAsync(ct);

        return new ToggleReviewMarkResponse(sessionId, questionId, answer.IsMarkedForReview);
    }
}
