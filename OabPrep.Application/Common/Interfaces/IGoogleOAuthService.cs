namespace OabPrep.Application.Common.Interfaces;

public record GoogleUserInfo(string Email, string Name, string Sub);

public interface IGoogleOAuthService
{
    /// <summary>
    /// Validates the Google idToken and returns the verified user info.
    /// Throws <see cref="Common.Exceptions.UnauthorizedException"/> if the token is invalid or the audience does not match.
    /// </summary>
    Task<GoogleUserInfo> ValidateAsync(string idToken, CancellationToken ct = default);
}
