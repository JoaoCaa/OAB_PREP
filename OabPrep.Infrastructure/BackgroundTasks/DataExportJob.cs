using System.IO.Compression;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Infrastructure.Persistence;

namespace OabPrep.Infrastructure.BackgroundTasks;

public sealed class DataExportJob : IDataExportJob
{
    private static readonly TimeSpan ExportLinkExpiry = TimeSpan.FromHours(48);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;
    private readonly IEmailService _emailService;
    private readonly ILogger<DataExportJob> _logger;

    public DataExportJob(
        ApplicationDbContext context,
        IStorageService storageService,
        IEmailService emailService,
        ILogger<DataExportJob> logger)
    {
        _context = context;
        _storageService = storageService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task RunAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[DATA-EXPORT] Starting export for user {UserId}", userId);

        var user = await _context.Users.FindAsync([userId], cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("[DATA-EXPORT] User {UserId} not found — aborting", userId);
            return;
        }

        var sessions = await _context.Sessions
            .Include(s => s.Answers)
            .Where(s => s.UserId == userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var exportData = new
        {
            ExportedAt = DateTime.UtcNow,
            Profile = new
            {
                user.Id,
                user.Name,
                user.Email,
                user.AvatarUrl,
                user.EmailConfirmed,
                user.CreatedAt
            },
            Sessions = sessions.Select(s => new
            {
                s.Id,
                Status = s.Status.ToString(),
                s.CorrectAnswers,
                s.CreatedAt,
                s.FinishedAt,
                Answers = s.Answers.Select(a => new
                {
                    a.QuestionId,
                    a.ChosenAlternativeId,
                    a.IsCorrect,
                    a.TimeSpentSeconds,
                    a.AnsweredAt,
                    a.IsMarkedForReview
                })
            })
        };

        using var memStream = new MemoryStream();
        using (var archive = new ZipArchive(memStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("data-export.json", CompressionLevel.Optimal);
            await using var entryStream = entry.Open();
            await JsonSerializer.SerializeAsync(entryStream, exportData, JsonOptions, cancellationToken);
        }

        memStream.Position = 0;
        var signedUrl = await _storageService.UploadExportAsync(userId, memStream, ExportLinkExpiry, cancellationToken);
        var expiresAt = DateTime.UtcNow.Add(ExportLinkExpiry);

        await _emailService.SendDataExportEmailAsync(user.Email, user.Name, signedUrl, expiresAt, cancellationToken);

        _logger.LogInformation("[DATA-EXPORT] Completed for user {UserId} | URL: {Url}", userId, signedUrl);
    }
}
