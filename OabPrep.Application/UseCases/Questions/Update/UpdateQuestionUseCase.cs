using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Application.UseCases.Questions.Create;
using OabPrep.Domain.Entities;
using OabPrep.Domain.Enums;

namespace OabPrep.Application.UseCases.Questions.Update;

public sealed class UpdateQuestionUseCase
{
    private readonly IQuestionRepository _repository;
    private readonly IAuditLogService _auditLogService;
    private readonly IApplicationDbContext _context;

    public UpdateQuestionUseCase(
        IQuestionRepository repository,
        IAuditLogService auditLogService,
        IApplicationDbContext context)
    {
        _repository = repository;
        _auditLogService = auditLogService;
        _context = context;
    }

    public async Task<QuestionDetailResponse> ExecuteAsync(
        UpdateQuestionCommand command,
        Guid adminUserId,
        CancellationToken ct = default)
    {
        var question = await _repository.FindByIdWithAlternativesAsync(command.Id, ct)
            ?? throw new NotFoundException("Question", command.Id);

        var alternatives = command.Alternatives
            .Select(a => new AlternativeData(a.Text, a.IsCorrect, a.Explanation))
            .ToList();

        question.Update(
            command.LawAreaId,
            command.Statement,
            command.Year,
            command.ExamEdition,
            command.Explanation,
            CreateQuestionUseCase.ToLegalRefsString(command.LegalRefs),
            (DifficultyLevel)command.Difficulty,
            alternatives);

        await _auditLogService.LogAsync(adminUserId, "QUESTION_UPDATED",
            $"QuestionId={command.Id}", ct);
        await _context.SaveChangesAsync(ct);

        return CreateQuestionUseCase.MapToDetail(question, lawAreaName: null);
    }
}
