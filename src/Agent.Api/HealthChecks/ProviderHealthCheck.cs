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
        var data = new Dictionary<string, object>();
        var issues = new List<string>();

        var hasAnyProvider = false;

        if (!string.IsNullOrEmpty(_options.Value.AnthropicApiKey))
        {
            data["claude"] = "configured";
            hasAnyProvider = true;
        }
        else
        {
            data["claude"] = "not configured";
        }

        if (!string.IsNullOrEmpty(_options.Value.OpenAIApiKey))
        {
            data["openai"] = "configured";
            hasAnyProvider = true;
        }
        else
        {
            data["openai"] = "not configured";
        }

        if (!string.IsNullOrEmpty(_options.Value.CursorApiKey))
        {
            data["cursor"] = "configured";
            hasAnyProvider = true;
        }
        else
        {
            data["cursor"] = "not configured";
        }

        if (!hasAnyProvider)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "No AI providers configured. At least one API key is required.",
                data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            "At least one AI provider is configured",
            data: data));
    }
}
