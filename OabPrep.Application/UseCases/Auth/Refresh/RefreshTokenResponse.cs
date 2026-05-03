namespace OabPrep.Application.UseCases.Auth.Refresh;

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn);
