namespace Agent.Api.Data.Entities;

public class AgentEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? WorkspacePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AgentRunEntity> Runs { get; set; } = new List<AgentRunEntity>();

    public Core.Models.Agent ToModel() => new()
    {
        Id = Id,
        Name = Name,
        Provider = Provider,
        WorkspacePath = WorkspacePath,
        CreatedAt = CreatedAt,
        UpdatedAt = UpdatedAt
    };

    public static AgentEntity FromModel(Core.Models.Agent model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Provider = model.Provider,
        WorkspacePath = model.WorkspacePath,
        CreatedAt = model.CreatedAt,
        UpdatedAt = model.UpdatedAt
    };
}
