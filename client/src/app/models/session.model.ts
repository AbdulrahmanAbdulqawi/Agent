export type AgentStatus = 'Idle' | 'Creating' | 'Running' | 'Finished' | 'Error' | 'Stopped';

export interface ConversationMessage {
  id: string;
  type: string;
  text: string;
  timestamp?: string;
}

export interface AgentSession {
  id: string;
  agentId: string;
  provider: string;
  runId: string;
  status: AgentStatus;
  messages: ConversationMessage[];
  summary?: string;
  error?: string;
  startedAt: string;
  completedAt?: string;
}
