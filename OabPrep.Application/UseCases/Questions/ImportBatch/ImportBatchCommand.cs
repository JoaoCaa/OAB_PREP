namespace OabPrep.Application.UseCases.Questions.ImportBatch;

public record ImportBatchCommand(
    IReadOnlyList<ImportBatchItem> Items);

public record ImportBatchItem(
    int LawAreaId,
    string Statement,
    int Year,
    string? ExamEdition,
    string? Explanation,
    IReadOnlyList<string>? LegalRefs,
    int Difficulty,
    IReadOnlyList<AlternativeImportItem> Alternatives);

public record AlternativeImportItem(
    string Letter,
    string Text,
    bool IsCorrect,
    string Explanation);