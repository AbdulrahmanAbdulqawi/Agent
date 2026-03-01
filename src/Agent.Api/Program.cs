using Agent.Api.Hubs;
using Agent.Api.Services;
using Agent.Core;
using Agent.Core.Models;
using Agent.Providers;
using Microsoft.AspNetCore.Cors;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapHub<AgentHub>("/hubs/agent");

app.Run();
