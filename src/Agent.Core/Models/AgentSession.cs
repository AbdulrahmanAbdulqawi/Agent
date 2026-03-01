namespace Agent.Core.Models;

public class AgentSession
{
    public string Id { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string RunId { get; set; } = string.Empty;
    public AgentStatus Status { get; set; } = AgentStatus.Idle;
    public List<ConversationMessage> Messages { get; set; } = new();
    public string? Summary { get; set; }
    public string? Error { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public Dictionary<string, object?> ExtraData { get; set; } = new();
}
