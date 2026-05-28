using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OabPrep.Infrastructure.Services.Llm;

internal sealed class GeminiLlmService : ILlmService
{
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";

    private readonly HttpClient _http;
    private readonly GeminiSettings _settings;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GeminiLlmService(HttpClient http, GeminiSettings settings)
    {
        _http = http;
        _settings = settings;
    }

    public async Task<LlmResponse> SendMessageAsync(LlmRequest request, CancellationToken ct = default)
    {
        var body = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = request.SystemPrompt } }
            },
            contents = request.Messages.Select(m => new
            {
                role = m.Role == "assistant" ? "model" : "user",
                parts = new[] { new { text = m.Content } }
            }).ToArray(),
            generationConfig = new
            {
                maxOutputTokens = request.MaxTokens,
                temperature = (double)request.Temperature
            }
        };

        return await WithRetryAsync(async () =>
        {
            var url = $"{BaseUrl}/{_settings.Model}:generateContent?key={_settings.ApiKey}";
            using var response = await _http.PostAsJsonAsync(url, body, JsonOpts, ct);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(ct);
                throw new LlmUnavailableException($"Gemini retornou {(int)response.StatusCode}: {err}");
            }

            using var doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

            var content = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            var tokens = doc.RootElement
                .GetProperty("usageMetadata")
                .GetProperty("totalTokenCount")
                .GetInt32();

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
