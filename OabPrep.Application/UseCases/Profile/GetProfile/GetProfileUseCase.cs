using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Application.UseCases.Profile.GetProfile;

public sealed class GetProfileUseCase
{
    private readonly IUserRepository _userRepository;

    public GetProfileUseCase(IUserRepository userRepository) =>
        _userRepository = userRepository;

    public async Task<GetProfileResponse> ExecuteAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.FindByIdAsync(userId, ct)
            ?? throw new NotFoundException("Usuário", userId);

        return new GetProfileResponse(
            user.Id,
            user.Name,
            user.Email,
            user.AvatarUrl,
            user.EmailConfirmed,
            user.CreatedAt);
    }
}
