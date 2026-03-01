using Agent.Api.HealthChecks;
using Agent.Api.Hubs;
using Agent.Api.Middleware;
using Agent.Api.Services;
using Agent.Api.Validators;
using Agent.Core;
using Agent.Core.Models;
using Agent.Providers;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Agent API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/agent-api-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        ));

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    builder.Services.AddSignalR();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<AgentValidator>();

    builder.Services.Configure<ProviderOptions>(builder.Configuration.GetSection("Providers"));

    builder.Services.AddHttpClient<CursorProvider>();
    builder.Services.AddHttpClient<ClaudeProvider>();
    builder.Services.AddHttpClient<OpenAIProvider>();

    builder.Services.AddSingleton<IAgentStore, AgentStore>();
    builder.Services.AddSingleton<IOrchestratorService, OrchestratorService>();
    builder.Services.AddSingleton<IEnumerable<IAgentProvider>>(sp => new IAgentProvider[]
    {
        sp.GetRequiredService<CursorProvider>(),
        sp.GetRequiredService<ClaudeProvider>(),
        sp.GetRequiredService<OpenAIProvider>()
    });

    builder.Services.AddTransient<GlobalExceptionHandler>();

    builder.Services.AddHealthChecks()
        .AddCheck<ProviderHealthCheck>("providers", tags: ["ready"])
        .AddCheck<StorageHealthCheck>("storage", tags: ["ready", "live"]);

    var app = builder.Build();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseMiddleware<GlobalExceptionHandler>();

    app.UseCors();
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live")
    });

    app.MapControllers();
    app.MapHub<AgentHub>("/hubs/agent");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
