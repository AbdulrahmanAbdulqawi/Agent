import { Component, OnInit, output, signal } from '@angular/core';
import { Agent } from '../../models/agent.model';
import { AgentService } from '../../services/agent.service';

@Component({
  selector: 'app-agent-list',
  standalone: true,
  imports: [],
  templateUrl: './agent-list.component.html',
  styleUrl: './agent-list.component.scss',
})
export class AgentListComponent implements OnInit {
  agents = signal<Agent[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  runAgent = output<Agent>();
  editAgent = output<Agent>();

  constructor(private agentService: AgentService) {}

  async ngOnInit() {
    await this.load();
  }

  async load() {
    this.loading.set(true);
    this.error.set(null);
    try {
      const list = await this.agentService.getAgents();
      this.agents.set(list);
    } catch (e) {
      this.error.set(e instanceof Error ? e.message : 'Failed to load agents');
    } finally {
      this.loading.set(false);
    }
  }

  async delete(agent: Agent) {
    if (!confirm(`Delete agent "${agent.name}"?`)) return;
    try {
      await this.agentService.deleteAgent(agent.id);
      await this.load();
    } catch (e) {
      this.error.set(e instanceof Error ? e.message : 'Failed to delete');
    }
  }

  onRun(agent: Agent) {
    this.runAgent.emit(agent);
  }

  onEdit(agent: Agent) {
    this.editAgent.emit(agent);
  }
}
