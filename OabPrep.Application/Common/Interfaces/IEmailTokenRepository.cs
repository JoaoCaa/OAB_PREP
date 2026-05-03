using OabPrep.Domain.Entities;

namespace OabPrep.Application.Common.Interfaces;

public interface IEmailTokenRepository
{
    Task AddAsync(EmailToken token, CancellationToken cancellationToken = default);
}
