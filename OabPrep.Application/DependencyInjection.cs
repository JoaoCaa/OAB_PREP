using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OabPrep.Application.UseCases.Auth.ConfirmEmail;
using OabPrep.Application.UseCases.Auth.Register;
using System.Reflection;

namespace OabPrep.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<RegisterUserUseCase>();
        services.AddScoped<ConfirmEmailUseCase>();

        return services;
    }
}
