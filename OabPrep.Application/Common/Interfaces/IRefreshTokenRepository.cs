using OabPrep.Domain.Entities;

namespace OabPrep.Application.Common.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task MarkAllAsRevokedAsync(Guid userId, CancellationToken cancellationToken = default);
}
