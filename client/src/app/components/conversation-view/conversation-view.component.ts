import { Component, input } from '@angular/core';
import { AgentSession, ConversationMessage } from '../../models/session.model';

@Component({
  selector: 'app-conversation-view',
  standalone: true,
  imports: [],
  templateUrl: './conversation-view.component.html',
  styleUrl: './conversation-view.component.scss',
})
export class ConversationViewComponent {
  session = input<AgentSession | null>(null);

  get messages(): ConversationMessage[] {
    return this.session()?.messages ?? [];
  }
}
