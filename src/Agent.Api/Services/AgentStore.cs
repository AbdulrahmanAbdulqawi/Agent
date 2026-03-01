using System.Text.Json;
using Agent.Core.Models;

namespace Agent.Api.Services;

public class AgentStore : IAgentStore
{
    private readonly string _dataPath;
    private readonly object _lock = new();

    public AgentStore(IHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath ?? AppDomain.CurrentDomain.BaseDirectory, "data");
        Directory.CreateDirectory(dataDir);
        _dataPath = Path.Combine(dataDir, "agents.json");
    }

    public IEnumerable<Core.Models.Agent> GetAll()
    {
        return Load().Values;
    }

    public Core.Models.Agent? Get(string id)
    {
        return Load().GetValueOrDefault(id);
    }

    public void Save(Core.Models.Agent agent)
    {
        lock (_lock)
        {
            var dict = Load();
            dict[agent.Id] = agent;
            SaveAll(dict);
        }
    }

    public void Delete(string id)
    {
        lock (_lock)
        {
            var dict = Load();
            dict.Remove(id);
            SaveAll(dict);
        }
    }

    private Dictionary<string, Core.Models.Agent> Load()
    {
        lock (_lock)
        {
            if (!File.Exists(_dataPath)) return new Dictionary<string, Core.Models.Agent>();
            try
            {
                var json = File.ReadAllText(_dataPath);
                var list = JsonSerializer.Deserialize<List<Core.Models.Agent>>(json) ?? new List<Core.Models.Agent>();
                return list.ToDictionary(a => a.Id, a => a);
            }
            catch
            {
                return new Dictionary<string, Core.Models.Agent>();
            }
        }
    }

    private void SaveAll(Dictionary<string, Core.Models.Agent> dict)
    {
        var list = dict.Values.ToList();
        var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_dataPath, json);
    }
}
