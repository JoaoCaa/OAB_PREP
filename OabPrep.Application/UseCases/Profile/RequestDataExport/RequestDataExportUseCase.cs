using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Application.UseCases.Profile.RequestDataExport;

public sealed class RequestDataExportUseCase
{
    private static readonly TimeSpan CooldownPeriod = TimeSpan.FromDays(30);

    private readonly IAuditLogService _auditLog;
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IApplicationDbContext _context;

    public RequestDataExportUseCase(
        IAuditLogService auditLog,
        IBackgroundTaskQueue taskQueue,
        IApplicationDbContext context)
    {
        _auditLog = auditLog;
        _taskQueue = taskQueue;
        _context = context;
    }

    public async Task ExecuteAsync(Guid userId, CancellationToken ct = default)
    {
        var lastExport = await _auditLog.GetLastActionDateAsync(userId, "DATA_EXPORT_REQUESTED", ct);
        if (lastExport.HasValue)
        {
            var elapsed = DateTime.UtcNow - lastExport.Value;
            if (elapsed < CooldownPeriod)
                throw new RateLimitExceededException(CooldownPeriod - elapsed);
        }

        await _auditLog.LogAsync(userId, "DATA_EXPORT_REQUESTED", cancellationToken: ct);
        await _context.SaveChangesAsync(ct);

        _taskQueue.Enqueue(async (sp, jobCt) =>
        {
            var job = (IDataExportJob)sp.GetService(typeof(IDataExportJob))!;
            await job.RunAsync(userId, jobCt);
        });
    }
}
