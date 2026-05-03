namespace OabPrep.Application.Common.Exceptions;

public sealed class InvalidTokenException : Exception
{
    public InvalidTokenException()
        : base("Token inválido ou expirado.") { }
}
