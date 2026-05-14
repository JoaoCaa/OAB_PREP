using FluentValidation;
using OabPrep.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace OabPrep.API.Middlewares;

public class GlobalExceptionMiddleware
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
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        if (exception is ValidationException validationEx)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var errors = validationEx.Errors
                .GroupBy(e => e.PropertyName.ToLowerInvariant())
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            var body = new
            {
                status = 400,
                title = "One or more validation errors occurred.",
                errors
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
            return;
        }

        if (exception is AccountLockedException lockedEx)
        {
            context.Response.StatusCode = 423;
            var body = new
            {
                status = 423,
                message = lockedEx.Message,
                retryAfterSeconds = (int)lockedEx.LockoutRemaining.TotalSeconds,
                timestamp = DateTime.UtcNow
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
            return;
        }

        if (exception is ChatLimitExceededException chatLimitEx)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            var body = new
            {
                status = 429,
                message = chatLimitEx.Message,
                limit = chatLimitEx.Limit,
                timestamp = DateTime.UtcNow
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
            return;
        }

        if (exception is RateLimitExceededException rateLimitEx)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.RetryAfter = ((int)rateLimitEx.RetryAfter.TotalSeconds).ToString();
            var body = new
            {
                status = 429,
                message = rateLimitEx.Message,
                retryAfterSeconds = (int)rateLimitEx.RetryAfter.TotalSeconds,
                timestamp = DateTime.UtcNow
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
            return;
        }

        var (statusCode, message) = exception switch
        {
            LlmUnavailableException ex => (HttpStatusCode.ServiceUnavailable, ex.Message),
            NotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
            FileTooLargeException ex => (HttpStatusCode.RequestEntityTooLarge, ex.Message),
            ConflictException ex => (HttpStatusCode.Conflict, ex.Message),
            ForbiddenException ex => (HttpStatusCode.Forbidden, ex.Message),
            SamePasswordException ex => (HttpStatusCode.BadRequest, ex.Message),
            UnauthorizedException ex => (HttpStatusCode.Unauthorized, ex.Message),
            InvalidTokenException ex => (HttpStatusCode.BadRequest, ex.Message),
            ArgumentException ex => (HttpStatusCode.BadRequest, ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Não autorizado."),
            _ => (HttpStatusCode.InternalServerError, "Erro interno do servidor.")
        };

        context.Response.StatusCode = (int)statusCode;

        var errorBody = new
        {
            status = (int)statusCode,
            message,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorBody, JsonOptions));
    }
}
