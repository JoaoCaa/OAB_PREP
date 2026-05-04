using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Enums;

namespace OabPrep.Application.UseCases.Sessions.SubmitAnswer;

public sealed class SubmitAnswerUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IApplicationDbContext _context;

    public SubmitAnswerUseCase(
        ISessionRepository sessionRepository,
        IQuestionRepository questionRepository,
        IApplicationDbContext context)
    {
        _sessionRepository = sessionRepository;
        _questionRepository = questionRepository;
        _context = context;
    }

    public async Task<SubmitAnswerResponse> ExecuteAsync(
        SubmitAnswerCommand command,
        Guid userId,
        CancellationToken ct = default)
    {
        var session = await _sessionRepository.FindByIdWithAnswersAsync(command.SessionId, ct)
            ?? throw new NotFoundException("Sessão", command.SessionId);

        if (session.UserId != userId)
            throw new ForbiddenException();

        if (session.Status != SessionStatus.InProgress)
            throw new ArgumentException("A sessão não está em progresso.");

        var sessionAnswer = session.Answers.FirstOrDefault(a => a.QuestionId == command.QuestionId)
            ?? throw new ArgumentException("Questão não pertence a esta sessão.");

        if (sessionAnswer.AnsweredAt.HasValue)
            throw new ConflictException("Esta questão já foi respondida.");

        var question = await _questionRepository.FindByIdWithAlternativesAsync(command.QuestionId, ct)
            ?? throw new NotFoundException("Questão", command.QuestionId);

        var chosen = question.Alternatives.FirstOrDefault(a => a.Id == command.SelectedAlternativeId)
            ?? throw new ArgumentException("Alternativa não pertence a esta questão.");

        var isCorrect = chosen.IsCorrect;

        sessionAnswer.Answer(command.SelectedAlternativeId, isCorrect, command.TimeSpentSeconds);

        if (isCorrect)
            session.IncrementCorrectAnswers();

        await _context.SaveChangesAsync(ct);

        var correctAlt = question.Alternatives.First(a => a.IsCorrect);

        var alternatives = question.Alternatives
            .Select(a => new AnswerAlternativeResponse(a.Id, a.Letter, a.Text, a.IsCorrect, a.Explanation))
            .ToList();

        return new SubmitAnswerResponse(
            isCorrect,
            correctAlt.Id,
            question.Explanation,
            ParseLegalRefs(question.LegalRefs),
            alternatives);
    }

    private static IReadOnlyList<string> ParseLegalRefs(string? legalRefs) =>
        string.IsNullOrEmpty(legalRefs)
            ? []
            : legalRefs.Split('|', StringSplitOptions.RemoveEmptyEntries);
}
