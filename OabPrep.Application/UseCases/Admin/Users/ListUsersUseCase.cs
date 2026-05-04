using OabPrep.Application.Common.Interfaces;
using OabPrep.Application.Common.Models;

namespace OabPrep.Application.UseCases.Admin.Users;

public sealed class ListUsersUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IUserPerformanceCacheRepository _cacheRepository;

    public ListUsersUseCase(
        IUserRepository userRepository,
        ISessionRepository sessionRepository,
        IUserPerformanceCacheRepository cacheRepository)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _cacheRepository = cacheRepository;
    }

    public async Task<PagedResult<AdminUserResponse>> ExecuteAsync(
        string? search,
        int page,
        int size,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        size = Math.Clamp(size, 1, 100);

        var (users, total) = await _userRepository.GetPagedAsync(search, page, size, ct);

        var userIds = users.Select(u => u.Id).ToList();

        var sessionCountsTask = _sessionRepository.GetSessionCountsByUserIdsAsync(userIds, ct);
        var answeredCountsTask = _cacheRepository.GetTotalAnsweredByUserIdsAsync(userIds, ct);

        await Task.WhenAll(sessionCountsTask, answeredCountsTask);

        var sessionCounts = sessionCountsTask.Result;
        var answeredCounts = answeredCountsTask.Result;

        var items = users.Select(u => new AdminUserResponse(
            u.Id, u.Name, u.Email, u.Role, u.IsActive, u.EmailConfirmed, u.AvatarUrl, u.CreatedAt,
            sessionCounts.GetValueOrDefault(u.Id),
            answeredCounts.GetValueOrDefault(u.Id)))
            .ToList();

        return new PagedResult<AdminUserResponse>(items, total, page, size);
    }
}
