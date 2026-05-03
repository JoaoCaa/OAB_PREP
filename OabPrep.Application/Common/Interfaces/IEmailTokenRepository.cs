using OabPrep.Domain.Entities;
using OabPrep.Domain.Enums;

namespace OabPrep.Application.Common.Interfaces;

public interface IEmailTokenRepository
{
    Task AddAsync(EmailToken token, CancellationToken cancellationToken = default);

    Task<EmailToken?> FindUnusedByHashAsync(
        string hashedToken,
        TokenType tokenType,
        CancellationToken cancellationToken = default);

    Task InvalidatePreviousByUserIdAsync(
        Guid userId,
        TokenType tokenType,
        CancellationToken cancellationToken = default);
}
