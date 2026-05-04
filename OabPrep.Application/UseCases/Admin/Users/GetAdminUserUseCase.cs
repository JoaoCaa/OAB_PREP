using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Application.UseCases.Admin.Users;

public sealed class GetAdminUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IUserPerformanceCacheRepository _cacheRepository;

    public GetAdminUserUseCase(
        IUserRepository userRepository,
        ISessionRepository sessionRepository,
        IUserPerformanceCacheRepository cacheRepository)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _cacheRepository = cacheRepository;
    }

    public async Task<AdminUserResponse> ExecuteAsync(Guid targetId, CancellationToken ct = default)
    {
        var user = await _userRepository.FindByIdAsync(targetId, ct)
            ?? throw new NotFoundException("Usuário", targetId);

        var userIds = new List<Guid> { targetId };
        var sessionCountsTask = _sessionRepository.GetSessionCountsByUserIdsAsync(userIds, ct);
        var answeredCountsTask = _cacheRepository.GetTotalAnsweredByUserIdsAsync(userIds, ct);

        await Task.WhenAll(sessionCountsTask, answeredCountsTask);

        return new AdminUserResponse(
            user.Id, user.Name, user.Email, user.Role, user.IsActive, user.EmailConfirmed, user.AvatarUrl,
            user.CreatedAt,
            sessionCountsTask.Result.GetValueOrDefault(targetId),
            answeredCountsTask.Result.GetValueOrDefault(targetId));
    }
}
