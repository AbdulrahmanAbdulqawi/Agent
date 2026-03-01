using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Agent.Core;
using Agent.Core.Models;
using Microsoft.Extensions.Options;

namespace Agent.Providers;

public class ClaudeProvider : IAgentProvider
{
    private readonly HttpClient _http;
    private readonly ProviderOptions _options;

    public string ProviderName => "Claude";

    public ClaudeProvider(HttpClient http, IOptions<ProviderOptions> options)
    {
        _http = http;
        _options = options.Value;
        _http.BaseAddress = new Uri("https://api.anthropic.com/");
        var key = _options.AnthropicApiKey ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? "";
        _http.DefaultRequestHeaders.Add("x-api-key", key);
        _http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<AgentSession> LaunchAsync(LaunchRequest request, string fullPrompt, CancellationToken ct = default)
    {
        var runId = Guid.NewGuid().ToString("N");
        var sessionId = $"claude-{runId}";

        var messages = new List<object> { new { role = "user", content = fullPrompt } };

        var payload = new
        {
            model = "claude-sonnet-4-20250514",
            max_tokens = 4096,
            system = PromptBuilder.SystemPrompt,
            messages
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("v1/messages", content, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ClaudeMessageResponse>(cancellationToken: ct);
        var text = ExtractText(result);
        var isComplete = CompletionDetection.IsComplete(text);

        var session = new AgentSession
        {
            Id = sessionId,
            RunId = runId,
            AgentId = request.AgentId,
            Provider = ProviderName,
            Status = isComplete ? AgentStatus.Finished : AgentStatus.Running,
            StartedAt = DateTime.UtcNow,
            Messages =
            [
                new ConversationMessage { Id = "1", Type = "user_message", Text = fullPrompt },
                new ConversationMessage { Id = "2", Type = "assistant_message", Text = text ?? "" }
            ]
        };

        session.ExtraData["Messages"] = messages;
        session.ExtraData["LastText"] = text;

        return session;
    }

    public async Task AddFollowUpAsync(AgentSession session, string followUpPrompt, CancellationToken ct = default)
    {
        var messages = (List<object>?)session.ExtraData.GetValueOrDefault("Messages") ?? new List<object>();
        var lastText = session.ExtraData.GetValueOrDefault("LastText") as string;

        if (!string.IsNullOrEmpty(lastText))
            messages.Add(new { role = "assistant", content = lastText });

        messages.Add(new { role = "user", content = followUpPrompt });

        var payload = new
        {
            model = "claude-sonnet-4-20250514",
            max_tokens = 4096,
            system = PromptBuilder.SystemPrompt,
            messages
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("v1/messages", content, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ClaudeMessageResponse>(cancellationToken: ct);
        var text = ExtractText(result);

        session.Messages.Add(new ConversationMessage { Id = Guid.NewGuid().ToString(), Type = "user_message", Text = followUpPrompt });
        session.Messages.Add(new ConversationMessage { Id = Guid.NewGuid().ToString(), Type = "assistant_message", Text = text ?? "" });

        session.ExtraData["Messages"] = messages;
        session.ExtraData["LastText"] = text;
        session.Status = CompletionDetection.IsComplete(text) ? AgentStatus.Finished : AgentStatus.Running;
    }

    public Task<AgentSession> GetStatusAsync(AgentSession session, CancellationToken ct = default) =>
        Task.FromResult(session);

    public Task StopAsync(AgentSession session, CancellationToken ct = default)
    {
        session.Status = AgentStatus.Stopped;
        return Task.CompletedTask;
    }

    private static string? ExtractText(ClaudeMessageResponse? r)
    {
        if (r?.Content == null) return null;
        var textBlock = r.Content.FirstOrDefault(c => c.Type == "text");
        return textBlock?.Text;
    }

    private class ClaudeMessageResponse
    {
        public List<ClaudeContentBlock>? Content { get; set; }
    }

    private class ClaudeContentBlock
    {
        public string? Type { get; set; }
        public string? Text { get; set; }
    }
}
