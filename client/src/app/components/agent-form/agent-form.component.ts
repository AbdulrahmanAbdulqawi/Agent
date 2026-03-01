import { Component, effect, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Agent } from '../../models/agent.model';

@Component({
  selector: 'app-agent-form',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './agent-form.component.html',
  styleUrl: './agent-form.component.scss',
})
export class AgentFormComponent {
  agent = input<Agent | null>(null);
  isEdit = input(false);

  saved = output<Agent>();
  cancelled = output<void>();

  name = signal('');
  provider = signal('Claude');
  workspacePath = signal('');

  providers = ['Claude', 'OpenAI', 'Cursor'];

  constructor() {
    effect(() => {
      const a = this.agent();
      if (a) {
        this.name.set(a.name);
        this.provider.set(a.provider);
        this.workspacePath.set(a.workspacePath ?? '');
      }
    });
  }

  onSubmit() {
    const a = this.agent();
    this.saved.emit({
      id: a?.id ?? '',
      name: this.name(),
      provider: this.provider(),
      workspacePath: this.workspacePath() || undefined,
    });
  }

  onCancel() {
    this.cancelled.emit();
  }
}
