using Agent.Api.Data;
using Agent.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AgentModel = Agent.Core.Models.Agent;

namespace Agent.Api.Services;

public class EfAgentStore : IAgentStore
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EfAgentStore(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public IEnumerable<AgentModel> GetAll()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AgentDbContext>();
        return db.Agents.AsNoTracking().Select(e => e.ToModel()).ToList();
    }

    public AgentModel? Get(string id)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AgentDbContext>();
        var entity = db.Agents.AsNoTracking().FirstOrDefault(a => a.Id == id);
        return entity?.ToModel();
    }

    public void Save(AgentModel agent)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AgentDbContext>();
        
        var existing = db.Agents.FirstOrDefault(a => a.Id == agent.Id);
        if (existing != null)
        {
            existing.Name = agent.Name;
            existing.Provider = agent.Provider;
            existing.WorkspacePath = agent.WorkspacePath;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            db.Agents.Add(AgentEntity.FromModel(agent));
        }
        
        db.SaveChanges();
    }

    public void Delete(string id)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AgentDbContext>();
        
        var entity = db.Agents.FirstOrDefault(a => a.Id == id);
        if (entity != null)
        {
            db.Agents.Remove(entity);
            db.SaveChanges();
        }
    }
}
