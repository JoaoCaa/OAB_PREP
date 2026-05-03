namespace OabPrep.Application.UseCases.Auth.Login;

public sealed record LoginCommand(string Email, string Password, bool RememberMe = false);
