using OabPrep.Domain.Entities;

namespace OabPrep.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<(IList<User> Users, int TotalCount)> GetPagedAsync(string? search, int page, int size, CancellationToken cancellationToken = default);
}
