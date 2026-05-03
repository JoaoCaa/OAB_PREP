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
}
