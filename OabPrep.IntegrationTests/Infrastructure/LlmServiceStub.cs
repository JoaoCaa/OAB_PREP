using OabPrep.Application.Common.Interfaces;

namespace OabPrep.IntegrationTests.Infrastructure;

/// <summary>Stub LLM that returns a canned response — used in integration tests to avoid real AI calls.</summary>
public sealed class LlmServiceStub : ILlmService
{
    public Task<LlmResponse> SendMessageAsync(LlmRequest request, CancellationToken ct = default) =>
        Task.FromResult(new LlmResponse("Resposta de teste.", 10, []));
}
