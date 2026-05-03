namespace OabPrep.Application.Common.Interfaces;

public interface ILoginAttemptService
{
    bool IsLockedOut(string email, out TimeSpan lockoutRemaining);
    void RecordFailure(string email);
    void Reset(string email);
}
