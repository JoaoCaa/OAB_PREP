namespace OabPrep.Application.UseCases.Sessions.GetSession;

public record SessionAlternativeStateResponse(int AlternativeId, string Letter, string Text, bool? IsCorrect);
