using System.Security.Cryptography;
using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;

namespace OabPrep.Application.UseCases.Auth.Refresh;

public sealed class RefreshTokenUseCase
{
    private static readonly TimeSpan AccessTokenExpiry = TimeSpan.FromHours(8);

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IApplicationDbContext _context;

    public RefreshTokenUseCase(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IJwtService jwtService,
        IApplicationDbContext context)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtService = jwtService;
        _context = context;
    }

    public async Task<RefreshTokenResponse> ExecuteAsync(RefreshTokenCommand command, CancellationToken ct = default)
    {
        var tokenHash = TryHashToken(command.RefreshToken)
            ?? throw new UnauthorizedException();

        var existingToken = await _refreshTokenRepository.FindByHashAsync(tokenHash, ct);

        if (existingToken is null || existingToken.RevokedAt is not null || existingToken.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedException();

        var user = await _userRepository.FindByIdAsync(existingToken.UserId, ct)
            ?? throw new UnauthorizedException();

        // Revoke old token — tracked by change tracker, committed atomically
        existingToken.Revoke();

        var (newTokenEntity, rawNewToken) = RefreshToken.Create(user.Id, DateTime.UtcNow.AddDays(30));
        await _refreshTokenRepository.AddAsync(newTokenEntity, ct);

        var accessToken = _jwtService.GenerateAccessToken(user, AccessTokenExpiry);

        await _context.SaveChangesAsync(ct);

        return new RefreshTokenResponse(accessToken, rawNewToken, (int)AccessTokenExpiry.TotalSeconds);
    }

    private static string? TryHashToken(string raw)
    {
        try
        {
            var bytes = Convert.FromHexString(raw);
            return Convert.ToHexString(SHA256.HashData(bytes));
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
