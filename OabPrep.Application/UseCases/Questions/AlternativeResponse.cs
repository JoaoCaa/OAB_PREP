namespace OabPrep.Application.UseCases.Questions;

public record AlternativeResponse(
    int Id,
    string Letter,
    string Text,
    bool IsCorrect,
    string Explanation);
