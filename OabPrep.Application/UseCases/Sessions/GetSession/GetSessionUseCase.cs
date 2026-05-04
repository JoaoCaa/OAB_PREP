using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Application.UseCases.Sessions.GetSession;

public sealed class GetSessionUseCase
{
    private readonly ISessionRepository _sessionRepository;

    public GetSessionUseCase(ISessionRepository sessionRepository) =>
        _sessionRepository = sessionRepository;

    public async Task<GetSessionResponse> ExecuteAsync(
        int sessionId,
        Guid userId,
        CancellationToken ct = default)
    {
        var session = await _sessionRepository.FindByIdFullAsync(sessionId, ct)
            ?? throw new NotFoundException("Sessão", sessionId);

        if (session.UserId != userId)
            throw new ForbiddenException();

        var answeredQuestionIds = session.Answers
            .Where(a => a.AnsweredAt.HasValue)
            .Select(a => a.QuestionId)
            .ToHashSet();

        var questions = session.Answers
            .Select(a =>
            {
                var answered = a.AnsweredAt.HasValue;
                var alternatives = a.Question!.Alternatives
                    .Select(alt => new SessionAlternativeStateResponse(
                        alt.Id,
                        alt.Letter,
                        alt.Text,
                        answered ? alt.IsCorrect : null))
                    .ToList();

                return new SessionQuestionStateResponse(
                    a.QuestionId,
                    a.Question.Statement,
                    a.Question.LawArea!.Name,
                    a.ChosenAlternativeId,
                    a.IsMarkedForReview,
                    alternatives);
            })
            .ToList();

        return new GetSessionResponse(
            session.Id,
            session.Status.ToString(),
            session.Answers.Count,
            answeredQuestionIds.Count,
            questions);
    }
}
