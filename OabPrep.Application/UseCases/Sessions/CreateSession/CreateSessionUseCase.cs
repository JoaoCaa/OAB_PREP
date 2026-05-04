using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;

namespace OabPrep.Application.UseCases.Sessions.CreateSession;

public sealed class CreateSessionUseCase
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IApplicationDbContext _context;

    public CreateSessionUseCase(
        IQuestionRepository questionRepository,
        ISessionRepository sessionRepository,
        IApplicationDbContext context)
    {
        _questionRepository = questionRepository;
        _sessionRepository = sessionRepository;
        _context = context;
    }

    public async Task<CreateSessionResponse> ExecuteAsync(
        CreateSessionCommand command,
        Guid userId,
        CancellationToken ct = default)
    {
        IReadOnlyCollection<int>? excludedIds = null;

        if (command.ExcludeAnswered)
            excludedIds = await _sessionRepository.GetCorrectlyAnsweredQuestionIdsAsync(userId, ct);

        var lawAreaIds = command.LawAreaIds is { Count: > 0 } ? command.LawAreaIds : null;

        var questions = await _questionRepository.GetActiveForSessionAsync(lawAreaIds, excludedIds, ct);

        if (questions.Count < command.QuestionCount)
            throw new ArgumentException(
                $"Não há questões suficientes disponíveis. Disponíveis: {questions.Count}, solicitadas: {command.QuestionCount}.");

        Shuffle(questions);
        var selected = questions.Take(command.QuestionCount).ToList();

        var session = Session.Create(userId, selected.Select(q => q.Id));
        await _sessionRepository.AddAsync(session, ct);
        await _context.SaveChangesAsync(ct);

        var questionDtos = selected
            .Select(q => MapToSessionQuestion(q))
            .ToList();

        return new CreateSessionResponse(session.Id, selected.Count, questionDtos);
    }

    private static SessionQuestionResponse MapToSessionQuestion(Question question)
    {
        var alternatives = question.Alternatives.ToList();
        Shuffle(alternatives);

        var altDtos = alternatives
            .Select(a => new SessionAlternativeResponse(a.Id, a.Letter, a.Text))
            .ToList();

        return new SessionQuestionResponse(
            question.Id,
            question.Statement,
            question.Year,
            question.ExamEdition,
            question.LawArea?.Name ?? string.Empty,
            (int)question.Difficulty,
            altDtos);
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
