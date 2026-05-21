using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;

namespace OabPrep.Application.UseCases.Auth.Login;

public sealed class LoginUseCase
{
    // Pre-computed BCrypt WF=12 hash used when the user is not found to prevent timing attacks
    // (BCrypt.Verify still runs full 2^12 rounds and returns false, keeping response time consistent)
    private const string DummyHash = "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TiGzmArfkX8lZdnJN.sVicJZbD56";

    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly ILoginAttemptService _loginAttemptService;
    private readonly IAuditLogService _auditLogService;
    private readonly IApplicationDbContext _context;

    public LoginUseCase(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        ILoginAttemptService loginAttemptService,
        IAuditLogService auditLogService,
        IApplicationDbContext context)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _loginAttemptService = loginAttemptService;
        _auditLogService = auditLogService;
        _context = context;
    }

    public async Task<LoginResponse> ExecuteAsync(LoginCommand command, CancellationToken ct = default)
    {
        var normalizedEmail = command.Email.ToLowerInvariant();

        if (_loginAttemptService.IsLockedOut(normalizedEmail, out var lockoutRemaining))
            throw new AccountLockedException(lockoutRemaining);

        var user = await _userRepository.FindByEmailAsync(normalizedEmail, ct);

        // Always verify password (even if user not found) to prevent timing-based user enumeration
        var hashToVerify = user?.PasswordHash ?? DummyHash;
        var passwordValid = await Task.Run(() => _passwordHasher.Verify(command.Password, hashToVerify), ct);

        if (user is null || !passwordValid || !user.IsActive || !user.EmailConfirmed)
        {
            _loginAttemptService.RecordFailure(normalizedEmail);

            if (user is not null)
            {
                await _auditLogService.LogAsync(user.Id, "LOGIN_FAILED", cancellationToken: ct);
                await _context.SaveChangesAsync(ct);
            }

            throw new UnauthorizedException();
        }

        _loginAttemptService.Reset(normalizedEmail);

        var expiresIn = command.RememberMe ? TimeSpan.FromDays(30) : TimeSpan.FromHours(8);
        var accessToken = _jwtService.GenerateAccessToken(user, expiresIn);

        var (refreshTokenEntity, rawRefreshToken) = RefreshToken.Create(
            user.Id,
            DateTime.UtcNow.AddDays(30));

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, ct);
        await _auditLogService.LogAsync(user.Id, "LOGIN_SUCCESS", cancellationToken: ct);
        await _context.SaveChangesAsync(ct);

       return new LoginResponse(
            accessToken,
            rawRefreshToken,
            (int)expiresIn.TotalSeconds,
            user.Id,
            user.Name,
            user.Email,
            user.Role);
    }
}
