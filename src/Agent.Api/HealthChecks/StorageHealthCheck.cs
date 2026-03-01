using Agent.Api.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Agent.Api.HealthChecks;

public class StorageHealthCheck : IHealthCheck
{
    private readonly IAgentStore _agentStore;
    private readonly IWebHostEnvironment _env;

    public StorageHealthCheck(IAgentStore agentStore, IWebHostEnvironment env)
    {
        _agentStore = agentStore;
        _env = env;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dataPath = Path.Combine(_env.ContentRootPath, "..", "..", "data");
            var data = new Dictionary<string, object>
            {
                ["dataPath"] = dataPath,
                ["exists"] = Directory.Exists(dataPath)
            };

            var agents = _agentStore.GetAll();
            data["agentCount"] = agents.Count();

            return Task.FromResult(HealthCheckResult.Healthy(
                "Storage is accessible",
                data: data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Storage check failed",
                exception: ex));
        }
    }
}
