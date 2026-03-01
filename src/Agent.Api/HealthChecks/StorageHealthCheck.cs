using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Agent.Api.HealthChecks;

public class StorageHealthCheck : IHealthCheck
{
    private readonly IWebHostEnvironment _environment;

    public StorageHealthCheck(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var dataPath = Path.Combine(_environment.ContentRootPath, "..", "..", "..", "data");
        var data = new Dictionary<string, object>
        {
            ["dataPath"] = dataPath
        };

        try
        {
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            var testFile = Path.Combine(dataPath, ".health-check");
            File.WriteAllText(testFile, DateTime.UtcNow.ToString("O"));
            File.Delete(testFile);

            data["writable"] = true;
            return Task.FromResult(HealthCheckResult.Healthy(
                "Storage is accessible and writable",
                data: data
            ));
        }
        catch (Exception ex)
        {
            data["writable"] = false;
            data["error"] = ex.Message;
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Storage check failed: {ex.Message}",
                exception: ex,
                data: data
            ));
        }
    }
}
