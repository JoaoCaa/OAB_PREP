using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Infrastructure.BackgroundTasks;
using OabPrep.Infrastructure.Email;
using OabPrep.Infrastructure.Persistence;
using OabPrep.Infrastructure.Repositories;
using OabPrep.Infrastructure.Security;
using OabPrep.Infrastructure.Services;
using OabPrep.Infrastructure.Services.Llm;
using OabPrep.Infrastructure.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace OabPrep.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmailTokenRepository, EmailTokenRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ILawAreaRepository, LawAreaRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IUserPerformanceCacheRepository, UserPerformanceCacheRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.AddScoped<SmtpEmailService>(sp =>
            new SmtpEmailService(
                sp.GetRequiredService<IOptions<EmailSettings>>().Value,
                sp.GetRequiredService<ILogger<SmtpEmailService>>()));
        services.AddScoped<SendGridEmailService>(sp =>
            new SendGridEmailService(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient("sendgrid"),
                sp.GetRequiredService<IOptions<EmailSettings>>().Value,
                sp.GetRequiredService<ILogger<SendGridEmailService>>()));
        services.AddScoped<IEmailService, BackgroundEmailService>();
        services.AddScoped<IStorageService, StorageServiceStub>();

        services.AddMemoryCache();
        services.AddSingleton<ILoginAttemptService, LoginAttemptService>();
        services.AddSingleton<IPasswordResetRateLimitService, PasswordResetRateLimitService>();

        services.AddScoped<IDataExportJob, DataExportJob>();
        services.AddScoped<IChatRepository, ChatRepository>();

        services.AddHostedService<BackgroundTaskProcessor>();
        services.AddHostedService<CleanupExpiredTokensService>();

        services.Configure<GoogleSettings>(configuration.GetSection("Google"));
        services.AddScoped<IGoogleOAuthService>(sp =>
            new GoogleOAuthService(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient("google"),
                sp.GetRequiredService<IOptions<GoogleSettings>>()));

        services.Configure<LlmSettings>(configuration.GetSection("Llm"));
        RegisterLlmService(services);

        return services;
    }

    private static void RegisterLlmService(IServiceCollection services)
    {
        services.AddHttpClient("sendgrid")
            .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(30));

        services.AddHttpClient("google")
            .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(10));

        services.AddHttpClient("llm")
            .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(30));

        services.AddScoped<ILlmService>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<LlmSettings>>().Value;
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var http = factory.CreateClient("llm");

            return settings.Provider switch
            {
                LlmProvider.Anthropic =>
                    new AnthropicLlmService(http, settings.Anthropic),
                LlmProvider.AzureOpenAI =>
                    new AzureOpenAiLlmService(http, settings.AzureOpenAI),
                _ =>
                    new OpenAiLlmService(http, settings.OpenAI)
            };
        });
    }
}
