import { Component, OnInit, output, signal } from '@angular/core';
import { Agent } from '../../models/agent.model';
import { AgentService } from '../../services/agent.service';
import { ToastService } from '../../services/toast.service';
import { LoadingSpinnerComponent } from '../loading-spinner/loading-spinner.component';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-agent-list',
  standalone: true,
  imports: [LoadingSpinnerComponent, ConfirmDialogComponent],
  templateUrl: './agent-list.component.html',
  styleUrl: './agent-list.component.scss',
})
export class AgentListComponent implements OnInit {
  agents = signal<Agent[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  deleteDialogOpen = signal(false);
  agentToDelete = signal<Agent | null>(null);
  deleting = signal(false);

  runAgent = output<Agent>();
  editAgent = output<Agent>();

  constructor(
    private agentService: AgentService,
    private toastService: ToastService
  ) {}

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
      const message = e instanceof Error ? e.message : 'Failed to load agents';
      this.error.set(message);
      this.toastService.error(message);
    } finally {
      this.loading.set(false);
    }
  }

  confirmDelete(agent: Agent) {
    this.agentToDelete.set(agent);
    this.deleteDialogOpen.set(true);
  }

  cancelDelete() {
    this.deleteDialogOpen.set(false);
    this.agentToDelete.set(null);
  }

  async performDelete() {
    const agent = this.agentToDelete();
    if (!agent) return;

    this.deleting.set(true);
    this.deleteDialogOpen.set(false);

    try {
      await this.agentService.deleteAgent(agent.id);
      this.toastService.success(`Agent "${agent.name}" deleted`);
      await this.load();
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Failed to delete';
      this.toastService.error(message);
    } finally {
      this.deleting.set(false);
      this.agentToDelete.set(null);
    }
  }

  onRun(agent: Agent) {
    this.runAgent.emit(agent);
  }

  onEdit(agent: Agent) {
    this.editAgent.emit(agent);
  }
}
