using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OabPrep.Application.Common.Interfaces;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace OabPrep.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddFluentValidationAutoValidation();
        services.AddSwaggerWithJwt();
        services.AddHealthChecks();
        services.AddCorsPolicies(configuration);
        services.AddJwtAuthentication(configuration);
        services.AddRateLimitingPolicies(configuration);
        services.AddAuthorization();

        return services;
    }

    private static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "OabPrep API",
                Version = "v1",
                Description = "API para preparação ao exame da OAB"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization. Informe: Bearer {token}",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    private static IServiceCollection AddCorsPolicies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
            options.AddPolicy("DefaultPolicy", policy =>
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            configuration["Jwt:Key"]
                            ?? throw new InvalidOperationException("Jwt:Key não configurado")))
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async ctx =>
                    {
                        var idClaim = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        if (!Guid.TryParse(idClaim, out var userId)) { ctx.Fail("Invalid token."); return; }
                        var repo = ctx.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                        var user = await repo.FindByIdAsync(userId, ctx.HttpContext.RequestAborted);
                        if (user is null || !user.IsActive) ctx.Fail("Conta inativa ou bloqueada.");
                    }
                };
            });

        return services;
    }

    private static IServiceCollection AddRateLimitingPolicies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Redis as optional distributed backing store.
        // When populated, swap PartitionByIp/PartitionByUser to Redis-backed limiters
        // (e.g. via the RedisRateLimiting NuGet package) for multi-instance deployments.
        var redisConnStr = configuration["Redis:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(redisConnStr))
        {
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(redisConnStr));
        }

        services.AddRateLimiter(options =>
        {
            options.OnRejected = async (context, token) =>
            {
                var response = context.HttpContext.Response;
                response.StatusCode = StatusCodes.Status429TooManyRequests;
                response.ContentType = "application/json";

                var retryAfterSeconds = 60;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    retryAfterSeconds = Math.Max(1, (int)retryAfter.TotalSeconds);

                response.Headers.RetryAfter = retryAfterSeconds.ToString();

                var body = new
                {
                    status = 429,
                    message = "Limite de requisições excedido. Tente novamente em instantes.",
                    retryAfterSeconds,
                    timestamp = DateTime.UtcNow
                };
                await response.WriteAsync(
                    JsonSerializer.Serialize(body, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }), token);
            };

            // auth-strict: 10 req/min by IP — login & forgot-password
            options.AddPolicy<string>(RateLimitPolicies.AuthStrict, ctx =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: ByIp(ctx),
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // chat: 30 req/min by userId — AI chat message endpoint
            options.AddPolicy<string>(RateLimitPolicies.Chat, ctx =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: ByUser(ctx),
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // api-standard: 200 req/min by userId — all other authenticated endpoints
            options.AddPolicy<string>(RateLimitPolicies.Standard, ctx =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: ByUser(ctx),
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 200,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // public: 60 req/min by IP — unauthenticated endpoints
            options.AddPolicy<string>(RateLimitPolicies.Public, ctx =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: ByIp(ctx),
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));
        });

        return services;
    }

    private static string ByIp(HttpContext ctx) =>
        $"ip:{ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

    private static string ByUser(HttpContext ctx)
    {
        var userId = ctx.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(userId) ? ByIp(ctx) : $"user:{userId}";
    }
}
