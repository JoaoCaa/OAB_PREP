namespace OabPrep.Application.Common.Exceptions;

public sealed class ChatLimitExceededException : Exception
{
    public int Limit { get; }

    public ChatLimitExceededException(int limit)
        : base($"Limite de {limit} mensagens por questão atingido.")
    {
        Limit = limit;
    }
}
