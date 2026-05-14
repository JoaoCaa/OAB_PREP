using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OabPrep.Infrastructure.Services.Llm;

internal sealed class OpenAiLlmService : ILlmService
{
    private readonly HttpClient _http;
    private readonly OpenAiSettings _settings;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OpenAiLlmService(HttpClient http, OpenAiSettings settings)
    {
        _http = http;
        _settings = settings;
    }

    public async Task<LlmResponse> SendMessageAsync(LlmRequest request, CancellationToken ct = default)
    {
        var body = new
        {
            model = _settings.Model,
            messages = BuildMessages(request),
            max_tokens = request.MaxTokens,
            temperature = request.Temperature
        };

        return await WithRetryAsync(async () =>
        {
            var url = $"{_settings.BaseUrl.TrimEnd('/')}/chat/completions";
            using var response = await _http.PostAsJsonAsync(url, body, JsonOpts, ct);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(ct);
                throw new LlmUnavailableException($"OpenAI retornou {(int)response.StatusCode}: {err}");
            }

            using var doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            var tokens = doc.RootElement
                .GetProperty("usage")
                .GetProperty("total_tokens")
                .GetInt32();

            return new LlmResponse(content, tokens, LegalRefsExtractor.Extract(content));
        });
    }

    private static object[] BuildMessages(LlmRequest request)
    {
        var msgs = new List<object>
        {
            new { role = "system", content = request.SystemPrompt }
        };
        msgs.AddRange(request.Messages.Select(m => new { role = m.Role, content = m.Content }));
        return msgs.ToArray();
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
