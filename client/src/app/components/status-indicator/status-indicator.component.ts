import { Component, input } from '@angular/core';
import { AgentStatus } from '../../models/session.model';

@Component({
  selector: 'app-status-indicator',
  standalone: true,
  imports: [],
  templateUrl: './status-indicator.component.html',
  styleUrl: './status-indicator.component.scss',
})
export class StatusIndicatorComponent {
  status = input<AgentStatus>('Idle');
  summary = input<string | undefined>(undefined);
  error = input<string | undefined>(undefined);
}
