using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Application.UseCases.Auth.Login;
using OabPrep.Domain.Entities;

namespace OabPrep.Application.UseCases.Auth.OAuthGoogle;

public sealed class OAuthGoogleUseCase
{
    private readonly IGoogleOAuthService _googleOAuth;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly IAuditLogService _auditLogService;
    private readonly IApplicationDbContext _context;

    public OAuthGoogleUseCase(
        IGoogleOAuthService googleOAuth,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService,
        IAuditLogService auditLogService,
        IApplicationDbContext context)
    {
        _googleOAuth = googleOAuth;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _auditLogService = auditLogService;
        _context = context;
    }

    public async Task<LoginResponse> ExecuteAsync(OAuthGoogleCommand command, CancellationToken ct = default)
    {
        // Throws UnauthorizedException if invalid or audience mismatch
        var googleUser = await _googleOAuth.ValidateAsync(command.IdToken, ct);

        var normalizedEmail = googleUser.Email.ToLowerInvariant();
        var user = await _userRepository.FindByEmailAsync(normalizedEmail, ct);

        if (user is null)
        {
            user = User.CreateOAuth(googleUser.Name, normalizedEmail);
            await _userRepository.AddAsync(user, ct);
        }
        else
        {
            if (!user.IsActive)
                throw new UnauthorizedException();

            user.RecordLogin();
        }

        var accessToken = _jwtService.GenerateAccessToken(user, TimeSpan.FromHours(8));
        var (refreshTokenEntity, rawRefreshToken) = RefreshToken.Create(user.Id, DateTime.UtcNow.AddDays(30));

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, ct);
        await _auditLogService.LogAsync(user.Id, "OAUTH_GOOGLE_LOGIN", cancellationToken: ct);
        await _context.SaveChangesAsync(ct);

        return new LoginResponse(
            accessToken,
            rawRefreshToken,
            (int)TimeSpan.FromHours(8).TotalSeconds,
            user.Id,
            user.Name,
            user.Email,
            user.Role);
    }
}
