using Agent.Core.Models;

namespace Agent.Api.Services;

public interface IOrchestratorService
{
    Task<string> StartRunAsync(LaunchRequest request, CancellationToken ct = default);
    Task StopRunAsync(string runId, CancellationToken ct = default);
    AgentSession? GetSession(string runId);
}
