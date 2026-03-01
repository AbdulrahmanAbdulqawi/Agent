using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Agent.Core;
using Agent.Core.Models;
using Microsoft.Extensions.Options;

namespace Agent.Providers;

public class OpenAIProvider : IAgentProvider
{
    private readonly HttpClient _http;
    private readonly ProviderOptions _options;

    public string ProviderName => "OpenAI";

    public OpenAIProvider(HttpClient http, IOptions<ProviderOptions> options)
    {
        _http = http;
        _options = options.Value;
        _http.BaseAddress = new Uri("https://api.openai.com/");
        var key = _options.OpenAIApiKey ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
    }

    public async Task<AgentSession> LaunchAsync(LaunchRequest request, string fullPrompt, CancellationToken ct = default)
    {
        var runId = Guid.NewGuid().ToString("N");
        var sessionId = $"openai-{runId}";

        var messages = new List<object>
        {
            new { role = "system", content = PromptBuilder.SystemPrompt },
            new { role = "user", content = fullPrompt }
        };

        var payload = new
        {
            model = "gpt-4o",
            max_tokens = 4096,
            messages
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("v1/chat/completions", content, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenAIChatResponse>(cancellationToken: ct);
        var text = result?.Choices?[0]?.Message?.Content;
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
            model = "gpt-4o",
            max_tokens = 4096,
            messages
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("v1/chat/completions", content, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenAIChatResponse>(cancellationToken: ct);
        var text = result?.Choices?[0]?.Message?.Content;

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

    private class OpenAIChatResponse
    {
        public List<OpenAIChoice>? Choices { get; set; }
    }

    private class OpenAIChoice
    {
        public OpenAIMessage? Message { get; set; }
    }

    private class OpenAIMessage
    {
        public string? Content { get; set; }
    }
}
