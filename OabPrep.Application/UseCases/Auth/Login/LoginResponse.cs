namespace OabPrep.Application.UseCases.Auth.Login;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    Guid UserId,
    string Name,
    string Email,
    string Role);