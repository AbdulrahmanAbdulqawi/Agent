using Agent.Api.Hubs;
using Agent.Core;
using Agent.Core.Models;
using Microsoft.AspNetCore.SignalR;

namespace Agent.Api.Services;

public class OrchestratorService : IOrchestratorService
{
    private readonly IEnumerable<IAgentProvider> _providers;
    private readonly IHubContext<AgentHub> _hubContext;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrchestratorService> _logger;
    private readonly Dictionary<string, (AgentSession Session, LaunchRequest Request, CancellationTokenSource Cts)> _runs = new();
    private readonly Dictionary<string, AgentSession> _completed = new();
    private readonly object _lock = new();

    private const int MaxIterations = 20;
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(30);

    public OrchestratorService(
        IEnumerable<IAgentProvider> providers, 
        IHubContext<AgentHub> hubContext,
        IServiceScopeFactory scopeFactory,
        ILogger<OrchestratorService> logger)
    {
        _providers = providers;
        _hubContext = hubContext;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<string> StartRunAsync(LaunchRequest request, CancellationToken ct = default)
    {
        var provider = GetProvider(request.Provider);
        if (provider == null)
            throw new InvalidOperationException($"Unknown provider: {request.Provider}");

        var fullPrompt = PromptBuilder.BuildFullPrompt(request.Goal, request.ExtraInfo);
        var session = await provider.LaunchAsync(request, fullPrompt, ct);

        var runId = session.RunId;
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(Timeout);

        lock (_lock)
        {
            _runs[runId] = (session, request, cts);
        }

        await SaveRunToHistoryAsync(session, request);

        _ = Task.Run(() => RunLoopAsync(runId, provider, request, fullPrompt, cts.Token), cts.Token);
        await EmitProgress(runId, "AgentStarted", session);

        return runId;
    }

    public async Task StopRunAsync(string runId, CancellationToken ct = default)
    {
        (AgentSession Session, LaunchRequest Request, CancellationTokenSource Cts) run;
        lock (_lock)
        {
            if (!_runs.TryGetValue(runId, out run)) return;
            _runs.Remove(runId);
        }

        run.Cts.Cancel();
        var provider = GetProvider(run.Session.Provider);
        if (provider != null)
            await provider.StopAsync(run.Session, ct);

        run.Session.Status = AgentStatus.Stopped;
        run.Session.CompletedAt = DateTime.UtcNow;
        await UpdateRunHistoryAsync(run.Session);
        await EmitProgress(runId, "AgentStopped", run.Session);
    }

    public AgentSession? GetSession(string runId)
    {
        lock (_lock)
        {
            if (_runs.TryGetValue(runId, out var run)) return run.Session;
            return _completed.TryGetValue(runId, out var completed) ? completed : null;
        }
    }

    private async Task SaveRunToHistoryAsync(AgentSession session, LaunchRequest request)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var runHistory = scope.ServiceProvider.GetService<IRunHistoryService>();
            if (runHistory != null)
            {
                await runHistory.SaveRunAsync(session, request.Goal, request.ExtraInfo);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save run to history: {RunId}", session.RunId);
        }
    }

    private async Task UpdateRunHistoryAsync(AgentSession session)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var runHistory = scope.ServiceProvider.GetService<IRunHistoryService>();
            if (runHistory != null)
            {
                await runHistory.UpdateRunAsync(session);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update run history: {RunId}", session.RunId);
        }
    }

    private IAgentProvider? GetProvider(string name) =>
        _providers.FirstOrDefault(p => string.Equals(p.ProviderName, name, StringComparison.OrdinalIgnoreCase));

    private async Task RunLoopAsync(string runId, IAgentProvider provider, LaunchRequest request, string fullPrompt, CancellationToken ct)
    {
        AgentSession session;
        lock (_lock)
        {
            if (!_runs.TryGetValue(runId, out var run)) return;
            session = run.Session;
        }

        var iteration = 0;
        var recentTexts = new List<string>();
        var isCursor = string.Equals(provider.ProviderName, "Cursor", StringComparison.OrdinalIgnoreCase);

        try
        {
            while (iteration < MaxIterations && session.Status != AgentStatus.Finished && session.Status != AgentStatus.Stopped && session.Status != AgentStatus.Error)
            {
                if (ct.IsCancellationRequested) break;

                if (isCursor)
                {
                    session = await provider.GetStatusAsync(session, ct);
                    lock (_lock)
                    {
                        if (_runs.TryGetValue(runId, out var r))
                            _runs[runId] = (session, r.Request, r.Cts);
                    }
                    await EmitProgress(runId, "AgentProgress", session);
                    if (session.Status == AgentStatus.Finished || session.Status == AgentStatus.Error)
                        break;
                }
                else
                {
                    var lastAssistant = session.Messages.LastOrDefault(m => m.Type == "assistant_message");
                    if (lastAssistant != null)
                    {
                        recentTexts.Add(lastAssistant.Text);
                        if (CompletionDetection.IsDoomLoop(recentTexts))
                        {
                            session.Status = AgentStatus.Finished;
                            session.Summary = "Stopped: repeated similar responses (doom loop)";
                            break;
                        }
                        if (recentTexts.Count > 5) recentTexts.RemoveAt(0);

                        if (CompletionDetection.IsComplete(lastAssistant.Text))
                        {
                            session.Status = AgentStatus.Finished;
                            session.CompletedAt = DateTime.UtcNow;
                            session.Summary = "Task completed";
                            break;
                        }
                    }

                    await provider.AddFollowUpAsync(session, "Based on your last response, continue. What is the next step? If you have finished the task, end your response with <DONE>.", ct);
                    await EmitProgress(runId, "AgentProgress", session);
                }

                iteration++;
                await Task.Delay(isCursor ? 3000 : 1000, ct);
            }

            if (session.Status != AgentStatus.Finished && session.Status != AgentStatus.Stopped)
                session.Status = iteration >= MaxIterations ? AgentStatus.Finished : session.Status;

            session.CompletedAt ??= DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            session.Status = AgentStatus.Error;
            session.Error = ex.Message;
            session.CompletedAt = DateTime.UtcNow;
        }
        finally
        {
            lock (_lock)
            {
                _runs.Remove(runId);
                _completed[runId] = session;
            }
            await UpdateRunHistoryAsync(session);
            await EmitProgress(runId, "AgentComplete", session);
        }
    }

    private async Task EmitProgress(string runId, string eventType, AgentSession session)
    {
        await _hubContext.Clients.Group($"run:{runId}").SendAsync(eventType, session);
    }
}
