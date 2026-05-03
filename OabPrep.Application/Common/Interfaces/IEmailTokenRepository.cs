using OabPrep.Domain.Entities;
using OabPrep.Domain.Enums;

namespace OabPrep.Application.Common.Interfaces;

public interface IEmailTokenRepository
{
    Task AddAsync(EmailToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna o token rastreado pelo EF Core se não tiver sido usado ainda (UsedAt IS NULL).
    /// Retorna null se não existir, já tiver sido usado ou o tipo não corresponder.
    /// </summary>
    Task<EmailToken?> FindUnusedByHashAsync(
        string hashedToken,
        TokenType tokenType,
        CancellationToken cancellationToken = default);
}
