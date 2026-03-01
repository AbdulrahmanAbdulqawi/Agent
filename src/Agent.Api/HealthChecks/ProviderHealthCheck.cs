using Agent.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Agent.Api.HealthChecks;

public class ProviderHealthCheck : IHealthCheck
{
    private readonly IOptions<ProviderOptions> _options;

    public ProviderHealthCheck(IOptions<ProviderOptions> options)
    {
        _options = options;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var options = _options.Value;
        var configuredProviders = new List<string>();
        var issues = new List<string>();

        if (!string.IsNullOrEmpty(options.AnthropicApiKey))
            configuredProviders.Add("Claude");
        else
            issues.Add("Anthropic API key not configured");

        if (!string.IsNullOrEmpty(options.OpenAIApiKey))
            configuredProviders.Add("OpenAI");
        else
            issues.Add("OpenAI API key not configured");

        if (!string.IsNullOrEmpty(options.CursorApiKey))
            configuredProviders.Add("Cursor");
        else
            issues.Add("Cursor API key not configured");

        var data = new Dictionary<string, object>
        {
            ["configuredProviders"] = configuredProviders,
            ["providerCount"] = configuredProviders.Count
        };

        if (configuredProviders.Count == 0)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "No AI providers configured",
                data: data
            ));
        }

        if (configuredProviders.Count < 3)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"Only {configuredProviders.Count}/3 providers configured: {string.Join(", ", configuredProviders)}",
                data: data
            ));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            "All providers configured",
            data: data
        ));
    }
}
