namespace OabPrep.Application.Common.Interfaces;

public interface IPasswordResetRateLimitService
{
    bool IsRateLimited(string email, out TimeSpan retryAfter);
    void RecordAttempt(string email);
}
