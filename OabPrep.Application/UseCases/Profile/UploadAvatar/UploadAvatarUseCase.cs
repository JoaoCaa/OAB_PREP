using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Application.UseCases.Profile.UploadAvatar;

public sealed class UploadAvatarUseCase
{
    private const long MaxSizeBytes = 2 * 1024 * 1024; // 2 MB
    private static readonly byte[] JpegMagic = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngMagic  = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private readonly IUserRepository _userRepository;
    private readonly IStorageService _storageService;
    private readonly IApplicationDbContext _context;

    public UploadAvatarUseCase(
        IUserRepository userRepository,
        IStorageService storageService,
        IApplicationDbContext context)
    {
        _userRepository = userRepository;
        _storageService = storageService;
        _context = context;
    }

    public async Task<UploadAvatarResponse> ExecuteAsync(
        Guid userId,
        Stream content,
        string contentType,
        long sizeBytes,
        CancellationToken ct = default)
    {
        if (sizeBytes > MaxSizeBytes)
            throw new FileTooLargeException("Avatar deve ter no máximo 2 MB.");

        var detectedType = await DetectMimeTypeAsync(content, ct);
        if (detectedType is null)
            throw new ArgumentException("Tipo de arquivo inválido. Apenas JPEG e PNG são aceitos.");

        var user = await _userRepository.FindByIdAsync(userId, ct)
            ?? throw new NotFoundException("Usuário", userId);

        var avatarUrl = await _storageService.UploadAvatarAsync(userId, content, detectedType, ct);
        user.UpdateAvatar(avatarUrl);

        await _context.SaveChangesAsync(ct);

        return new UploadAvatarResponse(avatarUrl);
    }

    private static async Task<string?> DetectMimeTypeAsync(Stream stream, CancellationToken ct)
    {
        var header = new byte[PngMagic.Length];
        var bytesRead = await stream.ReadAsync(header, ct);
        stream.Position = 0;

        if (bytesRead >= JpegMagic.Length && header.Take(JpegMagic.Length).SequenceEqual(JpegMagic))
            return "image/jpeg";

        if (bytesRead >= PngMagic.Length && header.Take(PngMagic.Length).SequenceEqual(PngMagic))
            return "image/png";

        return null;
    }
}
