using OabPrep.Application.UseCases.Questions;

namespace OabPrep.Application.UseCases.Questions.Update;

public record UpdateQuestionCommand(
    int Id,
    int LawAreaId,
    string Statement,
    int Year,
    string? ExamEdition,
    string? Explanation,
    IReadOnlyList<string>? LegalRefs,
    int Difficulty,
    IReadOnlyList<AlternativeCommandItem> Alternatives);
