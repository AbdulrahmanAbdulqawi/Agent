using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Agent.Core;
using Agent.Core.Models;
using Microsoft.Extensions.Options;

namespace Agent.Providers;

public class CursorProvider : IAgentProvider
{
    private readonly HttpClient _http;
    private readonly ProviderOptions _options;

    public string ProviderName => "Cursor";

    public CursorProvider(HttpClient http, IOptions<ProviderOptions> options)
    {
        _http = http;
        _options = options.Value;
        _http.BaseAddress = new Uri("https://api.cursor.com/");
        var key = _options.CursorApiKey ?? Environment.GetEnvironmentVariable("CURSOR_API_KEY") ?? "";
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{key}:")));
    }

    public async Task<AgentSession> LaunchAsync(LaunchRequest request, string fullPrompt, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.RepositoryUrl))
            throw new InvalidOperationException("Cursor requires a GitHub repository URL. Please provide it in Extra Info when running a Cursor agent.");

        var runId = Guid.NewGuid().ToString("N");
        var payload = new
        {
            prompt = new { text = fullPrompt },
            source = new
            {
                repository = request.RepositoryUrl.Trim(),
                @ref = request.Ref ?? "main"
            },
            target = new { autoCreatePr = false }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("v0/agents", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Cursor API error ({(int)response.StatusCode}): {response.ReasonPhrase}. {errorBody}");
        }

        var result = await response.Content.ReadFromJsonAsync<CursorAgentResponse>(cancellationToken: ct);
        if (result?.Id == null) throw new InvalidOperationException("Cursor API returned no agent ID");

        return new AgentSession
        {
            Id = result.Id,
            RunId = runId,
            AgentId = request.AgentId,
            Provider = ProviderName,
            Status = MapStatus(result.Status),
            StartedAt = DateTime.UtcNow
        };
    }

    public async Task AddFollowUpAsync(AgentSession session, string followUpPrompt, CancellationToken ct = default)
    {
        var payload = new { prompt = new { text = followUpPrompt } };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync($"v0/agents/{session.Id}/followup", content, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<AgentSession> GetStatusAsync(AgentSession session, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"v0/agents/{session.Id}", ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CursorAgentResponse>(cancellationToken: ct);
        if (result == null) return session;

        session.Status = MapStatus(result.Status);
        session.Summary = result.Summary;

        if (session.Status == AgentStatus.Finished || session.Status == AgentStatus.Error)
        {
            var convResponse = await _http.GetAsync($"v0/agents/{session.Id}/conversation", ct);
            if (convResponse.IsSuccessStatusCode)
            {
                var conv = await convResponse.Content.ReadFromJsonAsync<CursorConversationResponse>(cancellationToken: ct);
                if (conv?.Messages != null)
                {
                    session.Messages = conv.Messages.Select(m => new ConversationMessage
                    {
                        Id = m.Id ?? Guid.NewGuid().ToString(),
                        Type = m.Type ?? "unknown",
                        Text = m.Text ?? ""
                    }).ToList();
                }
            }
        }

        return session;
    }

    public async Task StopAsync(AgentSession session, CancellationToken ct = default)
    {
        await _http.PostAsync($"v0/agents/{session.Id}/stop", null, ct);
        session.Status = AgentStatus.Stopped;
    }

    private static AgentStatus MapStatus(string? status) => status?.ToUpperInvariant() switch
    {
        "CREATING" => AgentStatus.Creating,
        "RUNNING" => AgentStatus.Running,
        "FINISHED" => AgentStatus.Finished,
        _ => AgentStatus.Idle
    };

    private class CursorAgentResponse
    {
        public string? Id { get; set; }
        public string? Status { get; set; }
        public string? Summary { get; set; }
    }

    private class CursorConversationResponse
    {
        public List<CursorMessage>? Messages { get; set; }
    }

    private class CursorMessage
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public string? Text { get; set; }
    }
}
