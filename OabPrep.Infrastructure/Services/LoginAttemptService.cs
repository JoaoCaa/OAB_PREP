using Microsoft.Extensions.Caching.Memory;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Infrastructure.Services;

public sealed class LoginAttemptService : ILoginAttemptService
{
    private const int MaxFailures = 5;
    private static readonly TimeSpan FailureWindow = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private sealed record AttemptState(int Failures, DateTime? LockedUntil);

    private readonly IMemoryCache _cache;

    public LoginAttemptService(IMemoryCache cache) => _cache = cache;

    public bool IsLockedOut(string email, out TimeSpan lockoutRemaining)
    {
        if (_cache.TryGetValue(CacheKey(email), out AttemptState? state)
            && state?.LockedUntil is { } until
            && until > DateTime.UtcNow)
        {
            lockoutRemaining = until - DateTime.UtcNow;
            return true;
        }

        lockoutRemaining = TimeSpan.Zero;
        return false;
    }

    public void RecordFailure(string email)
    {
        var key = CacheKey(email);
        var current = _cache.TryGetValue(key, out AttemptState? existing)
            ? existing!
            : new AttemptState(0, null);

        var newFailures = current.Failures + 1;

        if (newFailures >= MaxFailures)
        {
            var lockedUntil = DateTime.UtcNow.Add(LockoutDuration);
            _cache.Set(key, new AttemptState(newFailures, lockedUntil),
                new MemoryCacheEntryOptions { AbsoluteExpiration = lockedUntil });
        }
        else
        {
            _cache.Set(key, new AttemptState(newFailures, null),
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.Add(FailureWindow)
                });
        }
    }

    public void Reset(string email) => _cache.Remove(CacheKey(email));

    private static string CacheKey(string email) => $"login_attempts:{email.ToLowerInvariant()}";
}
