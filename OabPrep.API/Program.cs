using OabPrep.API.Extensions;
using OabPrep.API.Middlewares;
using OabPrep.Application;
using OabPrep.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Map flat EMAIL_* env vars (Docker/cloud convention) to the nested Email:* config keys.
// Equivalent nested form also works: Email__Provider, Email__SendGrid__ApiKey, etc.
var emailOverrides = new Dictionary<string, string?>();
void MapEnv(string envVar, string configKey)
{
    var v = Environment.GetEnvironmentVariable(envVar);
    if (v is not null) emailOverrides[configKey] = v;
}
MapEnv("EMAIL_PROVIDER",    "Email:Provider");
MapEnv("EMAIL_FROM",        "Email:From");
MapEnv("EMAIL_FROM_NAME",   "Email:FromName");
MapEnv("SENDGRID_API_KEY",  "Email:SendGrid:ApiKey");
MapEnv("SMTP_HOST",         "Email:Smtp:Host");
MapEnv("SMTP_PORT",         "Email:Smtp:Port");
MapEnv("SMTP_USERNAME",     "Email:Smtp:Username");
MapEnv("SMTP_PASSWORD",     "Email:Smtp:Password");
if (emailOverrides.Count > 0)
    builder.Configuration.AddInMemoryCollection(emailOverrides);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<AuditMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OabPrep API v1"));
}

app.UseHttpsRedirection();
app.UseCors("DefaultPolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
