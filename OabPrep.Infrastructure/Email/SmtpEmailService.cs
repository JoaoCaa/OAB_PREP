using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using OabPrep.Application.Common.Interfaces;
using Polly;
using Polly.Retry;

namespace OabPrep.Infrastructure.Email;

public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly ResiliencePipeline _retry;

    public SmtpEmailService(EmailSettings settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings;
        _logger = logger;
        _retry = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    _logger.LogWarning(args.Outcome.Exception,
                        "SMTP retry {Attempt}/3 after {Delay:s}s",
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
            EmailTemplates.EmailConfirmation(name, confirmUrl, UnsubscribeUrl), ct);
    }

    public Task SendPasswordResetEmailAsync(string to, string name, string resetToken, CancellationToken ct = default)
    {
        var resetUrl = $"{_settings.AppBaseUrl}/redefinir-senha?token={Uri.EscapeDataString(resetToken)}";
        return SendAsync(to, name, "Redefinição de senha — OAB Prep",
            EmailTemplates.PasswordReset(name, resetUrl, UnsubscribeUrl), ct);
    }

    public Task SendDataExportEmailAsync(string to, string name, string downloadUrl, DateTime expiresAt, CancellationToken ct = default) =>
        SendAsync(to, name, "Seus dados estão prontos — OAB Prep",
            EmailTemplates.DataExportReady(name, downloadUrl, expiresAt, UnsubscribeUrl), ct);

    public Task SendAccountBlockedEmailAsync(string to, string name, string supportUrl, CancellationToken ct = default) =>
        SendAsync(to, name, "Sua conta no OAB Prep foi suspensa",
            EmailTemplates.AccountBlocked(name, supportUrl, UnsubscribeUrl), ct);

    private string UnsubscribeUrl => $"{_settings.AppBaseUrl}/descadastro";

    private Task SendAsync(string to, string name, string subject, string htmlBody, CancellationToken ct) =>
        _retry.ExecuteAsync(async token =>
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.From));
            message.To.Add(new MailboxAddress(name, to));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            var secureOption = _settings.Smtp.UseSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTlsWhenAvailable;

            await client.ConnectAsync(_settings.Smtp.Host, _settings.Smtp.Port, secureOption, token);

            if (!string.IsNullOrEmpty(_settings.Smtp.Username))
                await client.AuthenticateAsync(_settings.Smtp.Username, _settings.Smtp.Password, token);

            await client.SendAsync(message, token);
            await client.DisconnectAsync(true, token);

            _logger.LogInformation("Email sent via SMTP to {Email} subject={Subject}", to, subject);
        }, ct).AsTask();
}
