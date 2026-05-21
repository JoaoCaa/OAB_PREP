using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;
using OabPrep.Domain.Enums;

namespace OabPrep.Application.UseCases.Questions.ImportBatch;

public record ImportBatchResult(int Imported, int Failed, IReadOnlyList<ImportBatchError> Errors);
public record ImportBatchError(int Index, string Reason);

public sealed class ImportBatchUseCase
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IApplicationDbContext _context;

    public ImportBatchUseCase(
        IQuestionRepository questionRepository,
        IApplicationDbContext context)
    {
        _questionRepository = questionRepository;
        _context = context;
    }

    public async Task<ImportBatchResult> ExecuteAsync(
        ImportBatchCommand command,
        Guid adminId,
        CancellationToken ct = default)
    {
        var errors = new List<ImportBatchError>();
        var imported = 0;

        for (int i = 0; i < command.Items.Count; i++)
        {
            var item = command.Items[i];
            try
            {
                // Validações básicas
                if (string.IsNullOrWhiteSpace(item.Statement))
                { errors.Add(new(i, "Enunciado vazio")); continue; }

                if (item.Alternatives == null || item.Alternatives.Count < 3)
                { errors.Add(new(i, "Mínimo 3 alternativas")); continue; }

                if (item.Alternatives.Count(a => a.IsCorrect) != 1)
                { errors.Add(new(i, "Deve ter exatamente 1 alternativa correta")); continue; }

                var alternatives = item.Alternatives.Select(a =>
                    new AlternativeData(a.Text, a.IsCorrect, a.Explanation)
                ).ToList();

                var question = Question.Create(
                    lawAreaId:    item.LawAreaId,
                    statement:    item.Statement,
                    year:         item.Year,
                    examEdition:  item.ExamEdition,
                    explanation:  item.Explanation ?? "",
                    legalRefs:    item.LegalRefs != null ? System.Text.Json.JsonSerializer.Serialize(item.LegalRefs) : null,
                    difficulty:   (DifficultyLevel)item.Difficulty,
                    alternatives: alternatives);

                await _questionRepository.AddAsync(question, ct);
                imported++;

                // Salva em lotes de 50
                if (imported % 50 == 0)
                    await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                errors.Add(new(i, ex.Message));
            }
        }

        await _context.SaveChangesAsync(ct);
        return new ImportBatchResult(imported, errors.Count, errors);
    }
}