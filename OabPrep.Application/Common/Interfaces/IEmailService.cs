namespace OabPrep.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendConfirmationEmailAsync(
        string to,
        string name,
        string confirmationToken,
        CancellationToken cancellationToken = default);
}
