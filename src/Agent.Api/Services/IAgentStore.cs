using Agent.Core.Models;

namespace Agent.Api.Services;

public interface IAgentStore
{
    IEnumerable<Core.Models.Agent> GetAll();
    Core.Models.Agent? Get(string id);
    void Save(Core.Models.Agent agent);
    void Delete(string id);
}
