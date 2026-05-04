using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;
using OabPrep.Domain.Enums;

namespace OabPrep.Application.UseCases.Questions.Create;

public sealed class CreateQuestionUseCase
{
    private readonly IQuestionRepository _repository;
    private readonly IAuditLogService _auditLogService;
    private readonly IApplicationDbContext _context;

    public CreateQuestionUseCase(
        IQuestionRepository repository,
        IAuditLogService auditLogService,
        IApplicationDbContext context)
    {
        _repository = repository;
        _auditLogService = auditLogService;
        _context = context;
    }

    public async Task<QuestionDetailResponse> ExecuteAsync(
        CreateQuestionCommand command,
        Guid adminUserId,
        CancellationToken ct = default)
    {
        var alternatives = command.Alternatives
            .Select(a => new AlternativeData(a.Text, a.IsCorrect, a.Explanation))
            .ToList();

        var legalRefs = ToLegalRefsString(command.LegalRefs);

        var question = Question.Create(
            command.LawAreaId,
            command.Statement,
            command.Year,
            command.ExamEdition,
            command.Explanation,
            legalRefs,
            (DifficultyLevel)command.Difficulty,
            alternatives);

        await _repository.AddAsync(question, ct);
        await _auditLogService.LogAsync(adminUserId, "QUESTION_CREATED",
            $"QuestionId={question.Id}, LawAreaId={command.LawAreaId}", ct);
        await _context.SaveChangesAsync(ct);

        return MapToDetail(question, lawAreaName: null);
    }

    internal static string? ToLegalRefsString(IReadOnlyList<string>? refs) =>
        refs is { Count: > 0 } ? string.Join("|", refs) : null;

    internal static IReadOnlyList<string> ParseLegalRefs(string? legalRefs) =>
        string.IsNullOrEmpty(legalRefs)
            ? []
            : legalRefs.Split('|', StringSplitOptions.RemoveEmptyEntries);

    internal static QuestionDetailResponse MapToDetail(Question question, string? lawAreaName)
    {
        var alternatives = question.Alternatives
            .Select(a => new AlternativeResponse(a.Id, a.Letter, a.Text, a.IsCorrect, a.Explanation))
            .ToList();

        return new QuestionDetailResponse(
            question.Id,
            question.LawAreaId,
            lawAreaName ?? question.LawArea?.Name ?? string.Empty,
            question.Statement,
            question.Year,
            question.ExamEdition,
            question.Explanation,
            ParseLegalRefs(question.LegalRefs),
            (int)question.Difficulty,
            question.IsActive,
            alternatives);
    }
}
