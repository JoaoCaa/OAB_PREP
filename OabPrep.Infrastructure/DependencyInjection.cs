using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

namespace OabPrep.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
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
        services.AddScoped<IEmailService, EmailServiceStub>();
        services.AddScoped<IStorageService, StorageServiceStub>();

        services.AddMemoryCache();
        services.AddSingleton<ILoginAttemptService, LoginAttemptService>();
        services.AddSingleton<IPasswordResetRateLimitService, PasswordResetRateLimitService>();

        services.AddScoped<IDataExportJob, DataExportJob>();
        services.AddScoped<IChatRepository, ChatRepository>();

        services.AddHostedService<BackgroundTaskProcessor>();
        services.AddHostedService<CleanupExpiredTokensService>();

        services.Configure<LlmSettings>(configuration.GetSection("Llm"));
        RegisterLlmService(services);

        return services;
    }

    private static void RegisterLlmService(IServiceCollection services)
    {
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
