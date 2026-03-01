import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Agent } from '../models/agent.model';
import { ExtraInfo } from '../models/extra-info.model';
import { AgentSession } from '../models/session.model';

const API_BASE = '/api/agents';

@Injectable({ providedIn: 'root' })
export class AgentService {
  private hubConnection: signalR.HubConnection | null = null;
  readonly session = signal<AgentSession | null>(null);
  readonly isConnected = signal(false);

  async getAgents(): Promise<Agent[]> {
    const res = await fetch(API_BASE);
    if (!res.ok) throw new Error('Failed to fetch agents');
    return res.json();
  }

  async getAgent(id: string): Promise<Agent | null> {
    const res = await fetch(`${API_BASE}/${id}`);
    if (res.status === 404) return null;
    if (!res.ok) throw new Error('Failed to fetch agent');
    return res.json();
  }

  async createAgent(agent: Partial<Agent>): Promise<Agent> {
    const res = await fetch(API_BASE, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(agent),
    });
    if (!res.ok) throw new Error('Failed to create agent');
    return res.json();
  }

  async updateAgent(id: string, agent: Partial<Agent>): Promise<Agent> {
    const res = await fetch(`${API_BASE}/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ ...agent, id }),
    });
    if (!res.ok) throw new Error('Failed to update agent');
    return res.json();
  }

  async deleteAgent(id: string): Promise<void> {
    const res = await fetch(`${API_BASE}/${id}`, { method: 'DELETE' });
    if (!res.ok) throw new Error('Failed to delete agent');
  }

  async runAgent(
    agentId: string,
    goal: string,
    extraInfo?: ExtraInfo,
    repositoryUrl?: string,
    ref?: string
  ): Promise<{ runId: string }> {
    const res = await fetch(`${API_BASE}/${agentId}/run`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ goal, extraInfo: extraInfo ?? {}, repositoryUrl, ref }),
    });
    if (!res.ok) {
      const text = await res.text();
      let msg = 'Failed to start run';
      try {
        const json = JSON.parse(text);
        msg = (json.message ?? json.detail ?? json.title ?? text) || msg;
      } catch {
        if (text && text.length < 500) msg = text;
      }
      throw new Error(msg);
    }
    return res.json();
  }

  async stopRun(runId: string): Promise<void> {
    const res = await fetch(`${API_BASE}/runs/${runId}/stop`, { method: 'POST' });
    if (!res.ok) throw new Error('Failed to stop run');
  }

  async getRun(runId: string): Promise<AgentSession | null> {
    const res = await fetch(`${API_BASE}/runs/${runId}`);
    if (res.status === 404) return null;
    if (!res.ok) throw new Error('Failed to fetch run');
    return res.json();
  }

  async connectToRun(runId: string): Promise<void> {
    await this.disconnect();
    const baseUrl = window.location.origin;
    const hubUrl = baseUrl.includes('4200') ? 'http://localhost:5050' : baseUrl;
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${hubUrl}/hubs/agent`, { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('AgentStarted', (session: AgentSession) => this.session.set(session));
    this.hubConnection.on('AgentProgress', (session: AgentSession) => this.session.set(session));
    this.hubConnection.on('AgentComplete', (session: AgentSession) => this.session.set(session));
    this.hubConnection.on('AgentStopped', (session: AgentSession) => this.session.set(session));
    this.hubConnection.on('AgentError', (session: AgentSession) => this.session.set(session));

    await this.hubConnection.start();
    this.isConnected.set(true);
    await this.hubConnection.invoke('JoinRun', runId);
  }

  async disconnect(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = null;
    }
    this.isConnected.set(false);
  }
}
