namespace OabPrep.Application.Common.Exceptions;

public sealed class RateLimitExceededException : Exception
{
    public TimeSpan RetryAfter { get; }

    public RateLimitExceededException(TimeSpan retryAfter)
        : base("Muitas solicitações. Tente novamente mais tarde.")
    {
        RetryAfter = retryAfter;
    }
}
