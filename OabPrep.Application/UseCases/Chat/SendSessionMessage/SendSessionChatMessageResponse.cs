namespace OabPrep.Application.UseCases.Chat.SendSessionMessage;

public record SendSessionChatMessageResponse(
    int MessageId,
    string Content,
    string[] LegalRefs,
    int TokensUsed);
