namespace OabPrep.Application.UseCases.Questions.GetList;

public record GetQuestionsQuery(
    int? AreaId,
    int? Year,
    int? Difficulty,
    string? Search,
    int Page,
    int PageSize);
