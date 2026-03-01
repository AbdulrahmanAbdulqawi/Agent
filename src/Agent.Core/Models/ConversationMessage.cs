namespace Agent.Core.Models;

public class ConversationMessage
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "user_message", "assistant_message"
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
