namespace OabPrep.Application.Common.Interfaces;

public interface IStorageService
{
    Task<string> UploadAvatarAsync(Guid userId, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task<string> UploadExportAsync(Guid userId, Stream content, TimeSpan expiry, CancellationToken cancellationToken = default);
}
