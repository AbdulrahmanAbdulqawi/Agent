using Agent.Core.Models;
using Agent.Api.Services;
using Microsoft.AspNetCore.Mvc;
using AgentEntity = Agent.Core.Models.Agent;

namespace Agent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly IAgentStore _agentStore;
    private readonly IOrchestratorService _orchestrator;

    public AgentsController(IAgentStore agentStore, IOrchestratorService orchestrator)
    {
        _agentStore = agentStore;
        _orchestrator = orchestrator;
    }

    [HttpGet]
    public ActionResult<IEnumerable<AgentEntity>> List()
    {
        return Ok(_agentStore.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<AgentEntity?> Get(string id)
    {
        var agent = _agentStore.Get(id);
        return agent is null ? NotFound() : Ok(agent);
    }

    [HttpPost]
    public ActionResult<AgentEntity> Create([FromBody] AgentEntity agent)
    {
        agent.Id = Guid.NewGuid().ToString("N");
        agent.CreatedAt = DateTime.UtcNow;
        agent.UpdatedAt = DateTime.UtcNow;
        _agentStore.Save(agent);
        return CreatedAtAction(nameof(Get), new { id = agent.Id }, agent);
    }

    [HttpPut("{id}")]
    public ActionResult<AgentEntity> Update(string id, [FromBody] AgentEntity agent)
    {
        var existing = _agentStore.Get(id);
        if (existing is null) return NotFound();
        agent.Id = id;
        agent.UpdatedAt = DateTime.UtcNow;
        _agentStore.Save(agent);
        return Ok(agent);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        if (_agentStore.Get(id) is null) return NotFound();
        _agentStore.Delete(id);
        return NoContent();
    }

    [HttpPost("{id}/run")]
    public async Task<ActionResult<RunResponse>> Run(string id, [FromBody] RunRequest request, CancellationToken ct)
    {
        var agent = _agentStore.Get(id);
        if (agent is null) return NotFound();

        var launchRequest = new LaunchRequest
        {
            AgentId = id,
            Goal = request.Goal,
            ExtraInfo = request.ExtraInfo ?? new ExtraInfo(),
            Provider = agent.Provider,
            RepositoryUrl = request.RepositoryUrl,
            Ref = request.Ref
        };

        try
        {
            var runId = await _orchestrator.StartRunAsync(launchRequest, ct);
            return Ok(new RunResponse { RunId = runId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("runs/{runId}/stop")]
    public async Task<IActionResult> StopRun(string runId, CancellationToken ct)
    {
        await _orchestrator.StopRunAsync(runId, ct);
        return Ok();
    }

    [HttpGet("runs/{runId}")]
    public ActionResult<AgentSession?> GetRun(string runId)
    {
        var session = _orchestrator.GetSession(runId);
        return session is null ? NotFound() : Ok(session);
    }
}

public class RunRequest
{
    public string Goal { get; set; } = string.Empty;
    public ExtraInfo? ExtraInfo { get; set; }
    public string? RepositoryUrl { get; set; }
    public string? Ref { get; set; }
}

public class RunResponse
{
    public string RunId { get; set; } = string.Empty;
}
