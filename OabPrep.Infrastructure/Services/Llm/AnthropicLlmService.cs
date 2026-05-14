using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OabPrep.Infrastructure.Services.Llm;

internal sealed class AnthropicLlmService : ILlmService
{
    private const string AnthropicVersion = "2023-06-01";
    private const string BaseUrl = "https://api.anthropic.com/v1/messages";

    private readonly HttpClient _http;
    private readonly AnthropicSettings _settings;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public AnthropicLlmService(HttpClient http, AnthropicSettings settings)
    {
        _http = http;
        _settings = settings;
    }

    public async Task<LlmResponse> SendMessageAsync(LlmRequest request, CancellationToken ct = default)
    {
        var body = new
        {
            model = _settings.Model,
            system = request.SystemPrompt,
            messages = request.Messages.Select(m => new { role = m.Role, content = m.Content }).ToArray(),
            max_tokens = request.MaxTokens
        };

        return await WithRetryAsync(async () =>
        {
            using var response = await _http.PostAsJsonAsync(BaseUrl, body, JsonOpts, ct);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(ct);
                throw new LlmUnavailableException($"Anthropic retornou {(int)response.StatusCode}: {err}");
            }

            using var doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

            var content = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            var usage = doc.RootElement.GetProperty("usage");
            var tokens = usage.GetProperty("input_tokens").GetInt32()
                       + usage.GetProperty("output_tokens").GetInt32();

            return new LlmResponse(content, tokens, LegalRefsExtractor.Extract(content));
        });
    }

    private static async Task<T> WithRetryAsync<T>(Func<Task<T>> operation)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (LlmUnavailableException) when (attempt < 2)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
            catch (HttpRequestException) when (attempt < 2)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
            catch (TaskCanceledException) when (attempt < 2)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
        }
    }
}
