using Agent.Core.Models;

namespace Agent.Core;

public interface IAgentProvider
{
    string ProviderName { get; }
    Task<AgentSession> LaunchAsync(LaunchRequest request, string fullPrompt, CancellationToken ct = default);
    Task AddFollowUpAsync(AgentSession session, string followUpPrompt, CancellationToken ct = default);
    Task<AgentSession> GetStatusAsync(AgentSession session, CancellationToken ct = default);
    Task StopAsync(AgentSession session, CancellationToken ct = default);
}
