using FluentValidation;
using OabPrep.Application.Common.Exceptions;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace OabPrep.API.Middlewares;

public sealed class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogError(ex,
                "Unhandled exception TraceId={TraceId} UserId={UserId} {Method} {Path}",
                traceId, userId, context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex, traceId);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, string traceId)
    {
        if (context.Response.HasStarted) return;

        context.Response.ContentType = "application/problem+json";

        if (exception is ValidationException validationEx)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var errors = validationEx.Errors
                .GroupBy(e => e.PropertyName.ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = ProblemTypeUri(400),
                title = "Validation Error",
                status = 400,
                detail = "One or more validation errors occurred.",
                traceId,
                errors
            }, JsonOptions));
            return;
        }

        if (exception is AccountLockedException lockedEx)
        {
            context.Response.StatusCode = 423;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = ProblemTypeUri(423),
                title = "Account Locked",
                status = 423,
                detail = lockedEx.Message,
                traceId,
                retryAfterSeconds = (int)lockedEx.LockoutRemaining.TotalSeconds
            }, JsonOptions));
            return;
        }

        if (exception is ChatLimitExceededException chatLimitEx)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = ProblemTypeUri(429),
                title = "Chat Limit Exceeded",
                status = 429,
                detail = chatLimitEx.Message,
                traceId,
                limit = chatLimitEx.Limit
            }, JsonOptions));
            return;
        }

        if (exception is RateLimitExceededException rateLimitEx)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.RetryAfter = ((int)rateLimitEx.RetryAfter.TotalSeconds).ToString();
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = ProblemTypeUri(429),
                title = "Too Many Requests",
                status = 429,
                detail = rateLimitEx.Message,
                traceId,
                retryAfterSeconds = (int)rateLimitEx.RetryAfter.TotalSeconds
            }, JsonOptions));
            return;
        }

        var (statusCode, title, detail) = exception switch
        {
            LlmUnavailableException ex  => (503, "Service Unavailable", ex.Message),
            NotFoundException ex        => (404, "Not Found", ex.Message),
            FileTooLargeException ex    => (413, "Payload Too Large", ex.Message),
            ConflictException ex        => (409, "Conflict", ex.Message),
            ForbiddenException ex       => (403, "Forbidden", ex.Message),
            SamePasswordException ex    => (400, "Bad Request", ex.Message),
            UnauthorizedException ex    => (401, "Unauthorized", ex.Message),
            InvalidTokenException ex    => (400, "Bad Request", ex.Message),
            ArgumentException ex        => (400, "Bad Request", ex.Message),
            UnauthorizedAccessException => (401, "Unauthorized", "Não autorizado."),
            _                           => (500, "Internal Server Error", "Erro interno do servidor.")
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            type = ProblemTypeUri(statusCode),
            title,
            status = statusCode,
            detail,
            traceId
        }, JsonOptions));
    }

    private static string ProblemTypeUri(int status) => status switch
    {
        400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
        403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        413 => "https://tools.ietf.org/html/rfc7231#section-6.5.11",
        423 => "https://tools.ietf.org/html/rfc2616#section-10.4.24",
        429 => "https://tools.ietf.org/html/rfc6585#section-4",
        503 => "https://tools.ietf.org/html/rfc7231#section-6.6.4",
        _   => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };
}
