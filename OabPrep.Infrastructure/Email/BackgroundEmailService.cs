using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Infrastructure.Email;

/// <summary>
/// Wraps the real email service so every send is enqueued as a fire-and-forget background task.
/// Failures are logged as warnings — callers never receive an exception from email sends.
/// </summary>
public sealed class BackgroundEmailService : IEmailService
{
    private readonly IBackgroundTaskQueue _queue;
    private readonly EmailProvider _provider;
    private readonly ILogger<BackgroundEmailService> _logger;

    public BackgroundEmailService(
        IBackgroundTaskQueue queue,
        IOptions<EmailSettings> settings,
        ILogger<BackgroundEmailService> logger)
    {
        _queue = queue;
        _provider = settings.Value.Provider;
        _logger = logger;
    }

    public Task SendConfirmationEmailAsync(string to, string name, string confirmationToken, CancellationToken ct = default)
    {
        Enqueue(to, async (inner, bgCt) =>
            await inner.SendConfirmationEmailAsync(to, name, confirmationToken, bgCt));
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string to, string name, string resetToken, CancellationToken ct = default)
    {
        Enqueue(to, async (inner, bgCt) =>
            await inner.SendPasswordResetEmailAsync(to, name, resetToken, bgCt));
        return Task.CompletedTask;
    }

    public Task SendDataExportEmailAsync(string to, string name, string downloadUrl, DateTime expiresAt, CancellationToken ct = default)
    {
        Enqueue(to, async (inner, bgCt) =>
            await inner.SendDataExportEmailAsync(to, name, downloadUrl, expiresAt, bgCt));
        return Task.CompletedTask;
    }

    public Task SendAccountBlockedEmailAsync(string to, string name, string supportUrl, CancellationToken ct = default)
    {
        Enqueue(to, async (inner, bgCt) =>
            await inner.SendAccountBlockedEmailAsync(to, name, supportUrl, bgCt));
        return Task.CompletedTask;
    }

    private void Enqueue(string recipient, Func<IEmailService, CancellationToken, Task> send)
    {
        var provider = _provider;
        _queue.Enqueue(async (sp, ct) =>
        {
            // Resolve the concrete implementation (not IEmailService to avoid recursion)
            IEmailService inner = provider == EmailProvider.SendGrid
                ? sp.GetRequiredService<SendGridEmailService>()
                : sp.GetRequiredService<SmtpEmailService>();

            try
            {
                await send(inner, ct);
            }
            catch (Exception ex)
            {
                sp.GetRequiredService<ILogger<BackgroundEmailService>>()
                    .LogError(ex, "Failed to send email to {Recipient}", recipient);
            }
        });
    }
}
