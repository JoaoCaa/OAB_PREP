using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Infrastructure.BackgroundTasks;
using OabPrep.Infrastructure.Email;
using OabPrep.Infrastructure.Persistence;
using OabPrep.Infrastructure.Repositories;
using OabPrep.Infrastructure.Security;
using OabPrep.Infrastructure.Services;

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
        services.AddScoped<IAuditLogService, AuditLogService>();

        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddScoped<IEmailService, EmailServiceStub>();

        services.AddHostedService<BackgroundTaskProcessor>();

        return services;
    }
}
