namespace OabPrep.Application.UseCases.Chat.GetHistory;

public record ChatQuestionContext(string Statement, string AreaName, string? LegalRefs);

public record ChatMessageItem(int Id, string Role, string Content, DateTime CreatedAt, string[] LegalRefs);

public record GetChatHistoryResponse(
    ChatQuestionContext QuestionContext,
    IList<ChatMessageItem> Messages,
    int MessageCount,
    int MaxMessages);
