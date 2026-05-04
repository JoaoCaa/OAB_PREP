using OabPrep.Domain.Entities;

namespace OabPrep.Application.Common.Interfaces;

public interface IUserPerformanceCacheRepository
{
    Task<IList<UserPerformanceCache>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserPerformanceCache cache, CancellationToken cancellationToken = default);
}
