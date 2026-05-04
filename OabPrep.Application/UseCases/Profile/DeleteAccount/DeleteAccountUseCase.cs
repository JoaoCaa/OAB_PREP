using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Application.UseCases.Profile.DeleteAccount;

public sealed class DeleteAccountUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IApplicationDbContext _context;

    public DeleteAccountUseCase(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IApplicationDbContext context)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _context = context;
    }

    public async Task ExecuteAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.FindByIdAsync(userId, ct)
            ?? throw new NotFoundException("Usuário", userId);

        user.Anonymize();
        await _refreshTokenRepository.MarkAllAsRevokedAsync(userId, ct);

        await _context.SaveChangesAsync(ct);
    }
}
