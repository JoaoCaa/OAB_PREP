namespace OabPrep.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendConfirmationEmailAsync(
        string to,
        string name,
        string confirmationToken,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetEmailAsync(
        string to,
        string name,
        string resetToken,
        CancellationToken cancellationToken = default);
}
