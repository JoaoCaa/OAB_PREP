using Microsoft.Extensions.Logging;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Infrastructure.Storage;

// Stub para BE-25 (implementação real com Azure Blob / S3). Retorna URL fake para testes manuais.
public sealed class StorageServiceStub : IStorageService
{
    private readonly ILogger<StorageServiceStub> _logger;

    public StorageServiceStub(ILogger<StorageServiceStub> logger) => _logger = logger;

    public Task<string> UploadAvatarAsync(
        Guid userId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://storage.stub/avatars/{userId}.{ExtensionFor(contentType)}";
        _logger.LogInformation("[STORAGE STUB] Avatar upload → {UserId} | {ContentType} | URL: {Url}",
            userId, contentType, url);
        return Task.FromResult(url);
    }

    public Task<string> UploadExportAsync(
        Guid userId,
        Stream content,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var expiresAt = DateTime.UtcNow.Add(expiry);
        var url = $"https://storage.stub/exports/{userId}/data-export.zip?expires={expiresAt:o}";
        _logger.LogInformation("[STORAGE STUB] Export upload → {UserId} | Expires: {ExpiresAt} | URL: {Url}",
            userId, expiresAt, url);
        return Task.FromResult(url);
    }

    private static string ExtensionFor(string contentType) =>
        contentType == "image/png" ? "png" : "jpg";
}
