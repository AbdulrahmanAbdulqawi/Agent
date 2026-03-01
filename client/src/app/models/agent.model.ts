export interface Agent {
  id: string;
  name: string;
  provider: string;
  workspacePath?: string;
  createdAt?: string;
  updatedAt?: string;
}
