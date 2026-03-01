using System.Text.Json;
using Agent.Core.Models;

namespace Agent.Api.Data.Entities;

public class AgentRunEntity
{
    public int Id { get; set; }
    public string AgentId { get; set; } = string.Empty;
    public string RunId { get; set; } = string.Empty;
    public string Status { get; set; } = "Idle";
    public string Provider { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public string? Summary { get; set; }
    public string? Error { get; set; }
    public string? ExtraInfoJson { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public AgentEntity? Agent { get; set; }
    public ICollection<ConversationMessageEntity> Messages { get; set; } = new List<ConversationMessageEntity>();

    public AgentSession ToSession()
    {
        var session = new AgentSession
        {
            RunId = RunId,
            AgentId = AgentId,
            Provider = Provider,
            Status = Enum.TryParse<AgentStatus>(Status, out var status) ? status : AgentStatus.Idle,
            Summary = Summary,
            Error = Error,
            StartedAt = StartedAt,
            CompletedAt = CompletedAt
        };

        foreach (var msg in Messages.OrderBy(m => m.Sequence))
        {
            session.Messages.Add(msg.ToModel());
        }

        return session;
    }

    public static AgentRunEntity FromSession(AgentSession session, string? goal = null, ExtraInfo? extraInfo = null)
    {
        var entity = new AgentRunEntity
        {
            RunId = session.RunId,
            AgentId = session.AgentId,
            Provider = session.Provider,
            Status = session.Status.ToString(),
            Summary = session.Summary,
            Error = session.Error,
            Goal = goal,
            ExtraInfoJson = extraInfo != null ? JsonSerializer.Serialize(extraInfo) : null,
            StartedAt = session.StartedAt,
            CompletedAt = session.CompletedAt
        };

        var sequence = 0;
        foreach (var msg in session.Messages)
        {
            entity.Messages.Add(ConversationMessageEntity.FromModel(msg, sequence++));
        }

        return entity;
    }

    public void UpdateFromSession(AgentSession session)
    {
        Status = session.Status.ToString();
        Summary = session.Summary;
        Error = session.Error;
        CompletedAt = session.CompletedAt;

        Messages.Clear();
        var sequence = 0;
        foreach (var msg in session.Messages)
        {
            Messages.Add(ConversationMessageEntity.FromModel(msg, sequence++));
        }
    }
}
