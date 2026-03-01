using Agent.Api.Data;
using Agent.Api.Data.Entities;
using Agent.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Agent.Api.Services;

public class RunHistoryService : IRunHistoryService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RunHistoryService> _logger;

    public RunHistoryService(IServiceScopeFactory scopeFactory, ILogger<RunHistoryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task SaveRunAsync(AgentSession session, string? goal = null, ExtraInfo? extraInfo = null, CancellationToken ct = default)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AgentDbContext>();

            var entity = AgentRunEntity.FromSession(session, goal, extraInfo);
            db.Runs.Add(entity);
            await db.SaveChangesAsync(ct);
            
            _logger.LogInformation("Saved run {RunId} for agent {AgentId}", session.RunId, session.AgentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save run {RunId}", session.RunId);
        }
    }

    public async Task UpdateRunAsync(AgentSession session, CancellationToken ct = default)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AgentDbContext>();

            var entity = await db.Runs
                .Include(r => r.Messages)
                .FirstOrDefaultAsync(r => r.RunId == session.RunId, ct);

            if (entity == null)
            {
                _logger.LogWarning("Run {RunId} not found for update", session.RunId);
                return;
            }

            entity.UpdateFromSession(session);
            await db.SaveChangesAsync(ct);
            
            _logger.LogDebug("Updated run {RunId}, status: {Status}", session.RunId, session.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update run {RunId}", session.RunId);
        }
    }

    public async Task<AgentSession?> GetRunAsync(string runId, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AgentDbContext>();

        var entity = await db.Runs
            .Include(r => r.Messages)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RunId == runId, ct);

        return entity?.ToSession();
    }

    public async Task<IEnumerable<AgentSession>> GetRunsForAgentAsync(string agentId, int limit = 20, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AgentDbContext>();

        var entities = await db.Runs
            .Include(r => r.Messages)
            .Where(r => r.AgentId == agentId)
            .OrderByDescending(r => r.StartedAt)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync(ct);

        return entities.Select(e => e.ToSession());
    }

    public async Task<IEnumerable<AgentSession>> GetRecentRunsAsync(int limit = 50, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AgentDbContext>();

        var entities = await db.Runs
            .Include(r => r.Messages)
            .OrderByDescending(r => r.StartedAt)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync(ct);

        return entities.Select(e => e.ToSession());
    }
}
