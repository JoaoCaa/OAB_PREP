namespace OabPrep.Application.Common.Exceptions;

public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Acesso negado.") : base(message) { }
}
