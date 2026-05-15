using Microsoft.Extensions.Logging;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Infrastructure.Email;

// Stub para BE-25 (implementação real de e-mail). Loga o envio para facilitar testes manuais.
public sealed class EmailServiceStub : IEmailService
{
    private readonly ILogger<EmailServiceStub> _logger;

    public EmailServiceStub(ILogger<EmailServiceStub> logger) => _logger = logger;

    public Task SendConfirmationEmailAsync(
        string to,
        string name,
        string confirmationToken,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL STUB] Confirmation email → {Email} ({Name}) | Token: {Token}",
            to, name, confirmationToken);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(
        string to,
        string name,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL STUB] Password reset email → {Email} ({Name}) | Token: {Token}",
            to, name, resetToken);

        return Task.CompletedTask;
    }

    public Task SendDataExportEmailAsync(
        string to,
        string name,
        string downloadUrl,
        DateTime expiresAt,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL STUB] Data export email → {Email} ({Name}) | URL: {Url} | Expires: {ExpiresAt:o}",
            to, name, downloadUrl, expiresAt);

        return Task.CompletedTask;
    }

    public Task SendAccountBlockedEmailAsync(
        string to,
        string name,
        string supportUrl,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL STUB] Account blocked email → {Email} ({Name}) | Support: {SupportUrl}",
            to, name, supportUrl);

        return Task.CompletedTask;
    }
}
