using Agent.Core.Models;

namespace Agent.Api.Data.Entities;

public class ConversationMessageEntity
{
    public int Id { get; set; }
    public int RunId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int Sequence { get; set; }

    public AgentRunEntity? Run { get; set; }

    public ConversationMessage ToModel() => new()
    {
        Type = Type,
        Text = Text,
        Timestamp = Timestamp
    };

    public static ConversationMessageEntity FromModel(ConversationMessage model, int sequence) => new()
    {
        Type = model.Type,
        Text = model.Text,
        Timestamp = model.Timestamp,
        Sequence = sequence
    };
}
