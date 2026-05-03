using Microsoft.Extensions.Caching.Memory;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Infrastructure.Services;

public sealed class PasswordResetRateLimitService : IPasswordResetRateLimitService
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

    private sealed record AttemptState(int Count, DateTime WindowEnd);

    private readonly IMemoryCache _cache;

    public PasswordResetRateLimitService(IMemoryCache cache) => _cache = cache;

    public bool IsRateLimited(string email, out TimeSpan retryAfter)
    {
        if (_cache.TryGetValue(CacheKey(email), out AttemptState? state)
            && state is not null
            && state.Count >= MaxAttempts
            && state.WindowEnd > DateTime.UtcNow)
        {
            retryAfter = state.WindowEnd - DateTime.UtcNow;
            return true;
        }

        retryAfter = TimeSpan.Zero;
        return false;
    }

    public void RecordAttempt(string email)
    {
        var key = CacheKey(email);

        // Mantém a janela existente se ainda não expirou, senão inicia nova
        var current = _cache.TryGetValue(key, out AttemptState? existing) && existing?.WindowEnd > DateTime.UtcNow
            ? existing
            : new AttemptState(0, DateTime.UtcNow.Add(Window));

        var updated = current with { Count = current.Count + 1 };

        _cache.Set(key, updated, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = updated.WindowEnd
        });
    }

    private static string CacheKey(string email) => $"pwd_reset:{email.ToLowerInvariant()}";
}
