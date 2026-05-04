namespace OabPrep.Application.Common.Interfaces;

public interface IAuditLogService
{
    /// <summary>
    /// Adiciona uma entrada de auditoria ao contexto sem salvar (SaveChangesAsync fica
    /// responsabilidade do use case, garantindo atomicidade).
    /// </summary>
    Task LogAsync(
        Guid userId,
        string action,
        string? details = null,
        CancellationToken cancellationToken = default);

    Task<DateTime?> GetLastActionDateAsync(
        Guid userId,
        string action,
        CancellationToken cancellationToken = default);
}
