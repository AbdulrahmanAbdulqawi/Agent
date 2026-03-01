namespace Agent.Core.Models;

public class LaunchRequest
{
    public string AgentId { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public ExtraInfo ExtraInfo { get; set; } = new();
    public string Provider { get; set; } = string.Empty;
    public string? RepositoryUrl { get; set; } // For Cursor: GitHub repo URL
    public string? Ref { get; set; } // For Cursor: branch, tag, or commit
}
