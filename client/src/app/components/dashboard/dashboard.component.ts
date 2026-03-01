import { Component, signal } from '@angular/core';
import { Agent } from '../../models/agent.model';
import { AgentService } from '../../services/agent.service';
import { AgentListComponent } from '../agent-list/agent-list.component';
import { AgentFormComponent } from '../agent-form/agent-form.component';
import { RunAgentComponent } from '../run-agent/run-agent.component';
import { ConversationViewComponent } from '../conversation-view/conversation-view.component';
import { StatusIndicatorComponent } from '../status-indicator/status-indicator.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    AgentListComponent,
    AgentFormComponent,
    RunAgentComponent,
    ConversationViewComponent,
    StatusIndicatorComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent {
  view = signal<'list' | 'create' | 'edit' | 'run'>('list');
  selectedAgent = signal<Agent | null>(null);
  runId = signal<string | null>(null);

  constructor(public agentService: AgentService) {}

  showCreate() {
    this.selectedAgent.set(null);
    this.view.set('create');
  }

  showEdit(agent: Agent) {
    this.selectedAgent.set(agent);
    this.view.set('edit');
  }

  showRun(agent: Agent) {
    this.selectedAgent.set(agent);
    this.runId.set(null);
    this.view.set('run');
  }

  onRunStarted(runId: string) {
    this.runId.set(runId);
    this.agentService.connectToRun(runId);
  }

  backToList() {
    this.view.set('list');
    this.selectedAgent.set(null);
    this.runId.set(null);
    this.agentService.disconnect();
  }

  async onAgentSaved(agent: Agent) {
    try {
      if (agent.id) {
        await this.agentService.updateAgent(agent.id, agent);
      } else {
        await this.agentService.createAgent(agent);
      }
      this.backToList();
    } catch (e) {
      console.error('Failed to save agent', e);
    }
  }

  onFormCancelled() {
    this.backToList();
  }
}
