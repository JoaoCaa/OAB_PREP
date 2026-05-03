namespace OabPrep.Application.UseCases.Auth.Register;

public sealed record RegisterUserCommand
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
    public bool AcceptedTerms { get; init; }
}
