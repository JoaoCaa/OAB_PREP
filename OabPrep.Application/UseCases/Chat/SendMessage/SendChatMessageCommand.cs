namespace OabPrep.Application.UseCases.Chat.SendMessage;

public record SendChatMessageCommand(
    string UserMessage,
    int? SessionId,
    int? QuestionId);
