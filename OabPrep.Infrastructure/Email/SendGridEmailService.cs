using Microsoft.Extensions.Logging;
using OabPrep.Application.Common.Interfaces;
using Polly;
using Polly.Retry;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OabPrep.Infrastructure.Email;

public sealed class SendGridEmailService : IEmailService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _httpClient;
    private readonly EmailSettings _settings;
    private readonly ILogger<SendGridEmailService> _logger;
    private readonly ResiliencePipeline _retry;

    public SendGridEmailService(HttpClient httpClient, EmailSettings settings, ILogger<SendGridEmailService> logger)
    {
        _httpClient = httpClient;
        _settings = settings;
        _logger = logger;
        _retry = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                OnRetry = args =>
                {
                    _logger.LogWarning(args.Outcome.Exception,
                        "SendGrid retry {Attempt}/3 after {Delay:s}s",
                        args.AttemptNumber + 1, args.RetryDelay.TotalSeconds);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public Task SendConfirmationEmailAsync(string to, string name, string confirmationToken, CancellationToken ct = default)
    {
        var confirmUrl = $"{_settings.AppBaseUrl}/confirmar-email?token={Uri.EscapeDataString(confirmationToken)}";
        return SendAsync(to, name, "Confirme seu e-mail — OAB Prep",
            EmailTemplates.EmailConfirmation(name, confirmUrl), ct);
    }

    public Task SendPasswordResetEmailAsync(string to, string name, string resetToken, CancellationToken ct = default)
    {
        var resetUrl = $"{_settings.AppBaseUrl}/redefinir-senha?token={Uri.EscapeDataString(resetToken)}";
        return SendAsync(to, name, "Redefinição de senha — OAB Prep",
            EmailTemplates.PasswordReset(name, resetUrl), ct);
    }

    public Task SendDataExportEmailAsync(string to, string name, string downloadUrl, DateTime expiresAt, CancellationToken ct = default) =>
        SendAsync(to, name, "Seus dados estão prontos — OAB Prep",
            EmailTemplates.DataExportReady(name, downloadUrl, expiresAt), ct);

    private Task SendAsync(string to, string name, string subject, string htmlBody, CancellationToken ct) =>
        _retry.ExecuteAsync(async token =>
        {
            var payload = new
            {
                personalizations = new[]
                {
                    new { to = new[] { new { email = to, name } } }
                },
                from = new { email = _settings.From, name = _settings.FromName },
                subject,
                content = new[] { new { type = "text/html", value = htmlBody } }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.SendGrid.ApiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, token);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Email sent via SendGrid to {Email} subject={Subject}", to, subject);
        }, ct).AsTask();
}
