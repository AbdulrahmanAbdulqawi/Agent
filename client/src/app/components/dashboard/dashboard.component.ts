import { Component, signal } from '@angular/core';
import { Agent } from '../../models/agent.model';
import { AgentService } from '../../services/agent.service';
import { ToastService } from '../../services/toast.service';
import { AgentListComponent } from '../agent-list/agent-list.component';
import { AgentFormComponent } from '../agent-form/agent-form.component';
import { RunAgentComponent } from '../run-agent/run-agent.component';
import { ConversationViewComponent } from '../conversation-view/conversation-view.component';
import { StatusIndicatorComponent } from '../status-indicator/status-indicator.component';
import { LoadingSpinnerComponent } from '../loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    AgentListComponent,
    AgentFormComponent,
    RunAgentComponent,
    ConversationViewComponent,
    StatusIndicatorComponent,
    LoadingSpinnerComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent {
  view = signal<'list' | 'create' | 'edit' | 'run'>('list');
  selectedAgent = signal<Agent | null>(null);
  runId = signal<string | null>(null);
  saving = signal(false);

  constructor(
    public agentService: AgentService,
    private toastService: ToastService
  ) {}

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
    this.toastService.info('Agent run started');
  }

  backToList() {
    this.view.set('list');
    this.selectedAgent.set(null);
    this.runId.set(null);
    this.agentService.disconnect();
  }

  async onAgentSaved(agent: Agent) {
    this.saving.set(true);
    try {
      if (agent.id) {
        await this.agentService.updateAgent(agent.id, agent);
        this.toastService.success(`Agent "${agent.name}" updated`);
      } else {
        await this.agentService.createAgent(agent);
        this.toastService.success(`Agent "${agent.name}" created`);
      }
      this.backToList();
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Failed to save agent';
      this.toastService.error(message);
    } finally {
      this.saving.set(false);
    }
  }

  onFormCancelled() {
    this.backToList();
  }
}
