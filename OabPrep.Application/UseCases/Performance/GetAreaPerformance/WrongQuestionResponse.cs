namespace OabPrep.Application.UseCases.Performance.GetAreaPerformance;

public record WrongQuestionResponse(int QuestionId, string Statement, DateTime AnsweredAt);
