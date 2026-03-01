namespace Agent.Core.Models;

public class ExtraInfo
{
    public string? ProjectPath { get; set; }
    public string? TechStack { get; set; }
    public string? Constraints { get; set; }
    public string? RelevantFiles { get; set; }
    public string? Notes { get; set; }

    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(ProjectPath) &&
        string.IsNullOrWhiteSpace(TechStack) &&
        string.IsNullOrWhiteSpace(Constraints) &&
        string.IsNullOrWhiteSpace(RelevantFiles) &&
        string.IsNullOrWhiteSpace(Notes);

    public string ToContextString()
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(ProjectPath))
            parts.Add($"- Project path: {ProjectPath}");
        if (!string.IsNullOrWhiteSpace(TechStack))
            parts.Add($"- Tech stack: {TechStack}");
        if (!string.IsNullOrWhiteSpace(Constraints))
            parts.Add($"- Constraints: {Constraints}");
        if (!string.IsNullOrWhiteSpace(RelevantFiles))
            parts.Add($"- Relevant files: {RelevantFiles}");
        if (!string.IsNullOrWhiteSpace(Notes))
            parts.Add($"- Notes: {Notes}");

        return parts.Count == 0 ? string.Empty : string.Join("\n", parts);
    }
}
