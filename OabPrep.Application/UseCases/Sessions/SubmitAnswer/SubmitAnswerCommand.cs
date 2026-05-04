namespace OabPrep.Application.UseCases.Sessions.SubmitAnswer;

public record SubmitAnswerCommand(
    int SessionId,
    int QuestionId,
    int SelectedAlternativeId,
    int? TimeSpentSeconds);
