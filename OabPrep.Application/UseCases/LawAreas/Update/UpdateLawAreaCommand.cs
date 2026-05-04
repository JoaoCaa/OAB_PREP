namespace OabPrep.Application.UseCases.LawAreas.Update;

public record UpdateLawAreaCommand(int Id, string Name, string? Description, string? IconUrl);
