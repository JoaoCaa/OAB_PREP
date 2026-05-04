namespace OabPrep.Application.UseCases.LawAreas.Create;

public record CreateLawAreaCommand(string Name, string? Description, string? IconUrl);
