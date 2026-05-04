using OabPrep.Application.UseCases.Questions;

namespace OabPrep.Application.UseCases.Questions.Create;

public record CreateQuestionCommand(
    int LawAreaId,
    string Statement,
    int Year,
    string? ExamEdition,
    string? Explanation,
    IReadOnlyList<string>? LegalRefs,
    int Difficulty,
    IReadOnlyList<AlternativeCommandItem> Alternatives);
