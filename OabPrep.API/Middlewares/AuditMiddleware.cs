using OabPrep.Application.Common.Interfaces;
using System.Security.Claims;
using System.Text.Json;

namespace OabPrep.API.Middlewares;

public sealed class AuditMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(
        RequestDelegate next,
        IBackgroundTaskQueue taskQueue,
        ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _taskQueue = taskQueue;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        finally
        {
            if (ShouldAudit(context.Request.Path))
                EnqueueAuditLog(context);
        }
    }

    private void EnqueueAuditLog(HttpContext context)
    {
        var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var parsedUserId = Guid.TryParse(userId, out var uid) ? uid : Guid.Empty;
        var action = $"{context.Request.Method} {context.Request.Path.Value}";
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var statusCode = context.Response.StatusCode;
        var details = JsonSerializer.Serialize(new { ip, userAgent, statusCode }, JsonOptions);

        _taskQueue.Enqueue(async (sp, ct) =>
        {
            using var scope = sp.CreateScope();
            try
            {
                var auditService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                await auditService.LogAsync(parsedUserId, action, details, ct);
                await dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuditMiddleware>>();
                logger.LogWarning(ex, "Failed to persist audit log for {Action}", action);
            }
        });
    }

    private static bool ShouldAudit(PathString path) =>
        path.StartsWithSegments("/api/v1/admin") ||
        path.Equals("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/api/v1/auth/logout", StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/api/v1/auth/forgot-password", StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/api/v1/auth/reset-password", StringComparison.OrdinalIgnoreCase);
}
