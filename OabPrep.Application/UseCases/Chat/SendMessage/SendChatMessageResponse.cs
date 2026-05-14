namespace OabPrep.Application.UseCases.Chat.SendMessage;

public record SendChatMessageResponse(
    int MessageId,
    string Content,
    string[] LegalRefs,
    int TokensUsed);
