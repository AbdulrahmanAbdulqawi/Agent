using Agent.Core.Models;

namespace Agent.Api.Services;

public interface IRunHistoryService
{
    Task SaveRunAsync(AgentSession session, string? goal = null, ExtraInfo? extraInfo = null, CancellationToken ct = default);
    Task UpdateRunAsync(AgentSession session, CancellationToken ct = default);
    Task<AgentSession?> GetRunAsync(string runId, CancellationToken ct = default);
    Task<IEnumerable<AgentSession>> GetRunsForAgentAsync(string agentId, int limit = 20, CancellationToken ct = default);
    Task<IEnumerable<AgentSession>> GetRecentRunsAsync(int limit = 50, CancellationToken ct = default);
}
