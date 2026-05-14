namespace OabPrep.Application.Common.Interfaces;

public record LlmMessage(string Role, string Content);

public record LlmRequest(
    string SystemPrompt,
    List<LlmMessage> Messages,
    int MaxTokens = 800,
    double Temperature = 0.3);

public record LlmResponse(
    string Content,
    int TokensUsed,
    string[] LegalRefsExtracted);

public interface ILlmService
{
    Task<LlmResponse> SendMessageAsync(LlmRequest request, CancellationToken ct = default);
}
