using Microsoft.EntityFrameworkCore;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;
using OabPrep.Infrastructure.Persistence;

namespace OabPrep.Infrastructure.Repositories;

public sealed class UserPerformanceCacheRepository : IUserPerformanceCacheRepository
{
    private readonly ApplicationDbContext _context;

    public UserPerformanceCacheRepository(ApplicationDbContext context) => _context = context;

    public async Task<IList<UserPerformanceCache>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await _context.UserPerformanceCaches
            .Where(c => c.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(
        UserPerformanceCache cache,
        CancellationToken cancellationToken = default) =>
        await _context.UserPerformanceCaches.AddAsync(cache, cancellationToken);
}
