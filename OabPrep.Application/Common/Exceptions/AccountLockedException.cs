namespace OabPrep.Application.Common.Exceptions;

public sealed class AccountLockedException : Exception
{
    public TimeSpan LockoutRemaining { get; }

    public AccountLockedException(TimeSpan lockoutRemaining)
        : base("Conta temporariamente bloqueada por excesso de tentativas.")
    {
        LockoutRemaining = lockoutRemaining;
    }
}
