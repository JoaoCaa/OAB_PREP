namespace OabPrep.Application.Common.Exceptions;

public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException() : base("E-mail ou senha inválidos.") { }
}
