using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Application.UseCases.Questions.Deactivate;

public sealed class DeactivateQuestionUseCase
{
    private readonly IQuestionRepository _repository;
    private readonly IAuditLogService _auditLogService;
    private readonly IApplicationDbContext _context;

    public DeactivateQuestionUseCase(
        IQuestionRepository repository,
        IAuditLogService auditLogService,
        IApplicationDbContext context)
    {
        _repository = repository;
        _auditLogService = auditLogService;
        _context = context;
    }

    public async Task ExecuteAsync(int id, Guid adminUserId, CancellationToken ct = default)
    {
        var question = await _repository.FindByIdWithAlternativesAsync(id, ct)
            ?? throw new NotFoundException("Question", id);

        question.Deactivate();
        await _auditLogService.LogAsync(adminUserId, "QUESTION_DEACTIVATED",
            $"QuestionId={id}", ct);
        await _context.SaveChangesAsync(ct);
    }
}
