namespace OabPrep.Application.Common.Exceptions;

public sealed class FileTooLargeException : Exception
{
    public FileTooLargeException(string message) : base(message) { }
}
