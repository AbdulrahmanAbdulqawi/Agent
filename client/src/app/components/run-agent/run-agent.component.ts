import { Component, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Agent } from '../../models/agent.model';
import { ExtraInfo } from '../../models/extra-info.model';
import { AgentService } from '../../services/agent.service';

@Component({
  selector: 'app-run-agent',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './run-agent.component.html',
  styleUrl: './run-agent.component.scss',
})
export class RunAgentComponent {
  agent = input.required<Agent>();
  runStarted = output<string>();

  goal = signal('');
  projectPath = signal('');
  techStack = signal('');
  constraints = signal('');
  relevantFiles = signal('');
  notes = signal('');
  repositoryUrl = signal('');
  ref = signal('main');

  extraInfoExpanded = signal(false);
  loading = signal(false);
  error = signal<string | null>(null);

  constructor(private agentService: AgentService) {}

  async onRun() {
    const g = this.goal().trim();
    if (!g) {
      this.error.set('Please enter a goal');
      return;
    }

    if (this.agent().provider === 'Cursor' && !this.repositoryUrl()?.trim()) {
      this.error.set('Cursor requires a GitHub repository URL. Please enter it above.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const extraInfo: ExtraInfo = {};
    if (this.projectPath()) extraInfo.projectPath = this.projectPath();
    if (this.techStack()) extraInfo.techStack = this.techStack();
    if (this.constraints()) extraInfo.constraints = this.constraints();
    if (this.relevantFiles()) extraInfo.relevantFiles = this.relevantFiles();
    if (this.notes()) extraInfo.notes = this.notes();

    try {
      const { runId } = await this.agentService.runAgent(
        this.agent().id,
        g,
        Object.keys(extraInfo).length ? extraInfo : undefined,
        this.agent().provider === 'Cursor' ? (this.repositoryUrl()?.trim() || '') : undefined,
        this.agent().provider === 'Cursor' ? (this.ref()?.trim() || 'main') : undefined
      );
      this.runStarted.emit(runId);
    } catch (e) {
      this.error.set(e instanceof Error ? e.message : 'Failed to start run');
    } finally {
      this.loading.set(false);
    }
  }

  toggleExtraInfo() {
    this.extraInfoExpanded.update((v) => !v);
  }
}
