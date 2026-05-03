namespace OabPrep.Application.Common.Exceptions;

public sealed class SamePasswordException : Exception
{
    public SamePasswordException() : base("A nova senha não pode ser igual à senha atual.") { }
}
