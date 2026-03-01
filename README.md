# Autonomous AI Coding Agent

An Angular + .NET web application that runs locally. Create agents, add goals and extra context, then run them autonomously with Claude, OpenAI, or Cursor.

## Prerequisites

- .NET 8 SDK
- Node.js 18+
- API keys for at least one provider: Anthropic, OpenAI, or Cursor

## Setup

### 1. Configure API Keys

Create `src/Agent.Api/appsettings.Development.json` or use User Secrets:

```bash
cd src/Agent.Api
dotnet user-secrets set "Providers:AnthropicApiKey" "your-anthropic-key"
dotnet user-secrets set "Providers:OpenAIApiKey" "your-openai-key"
dotnet user-secrets set "Providers:CursorApiKey" "your-cursor-key"
```

Or set environment variables:
- `ANTHROPIC_API_KEY`
- `OPENAI_API_KEY`
- `CURSOR_API_KEY`

### 2. Run the Backend

```bash
cd src/Agent.Api
dotnet run
```

API runs at http://localhost:5050

### 3. Run the Frontend

```bash
cd client
npm install
npm start
```

Angular runs at http://localhost:4200 with proxy to the API.

## Usage

1. **Create Agent** – Name it, pick AI provider (Claude / OpenAI / Cursor), optionally set workspace path
2. **Add Goal** – Clear description of what the agent should do
3. **Add Extra Info** – Optional: project path, tech stack, constraints, relevant files, notes
4. **Run** – Start the agent; it runs autonomously until done
5. **Monitor** – Real-time progress and conversation via SignalR

## Cursor Notes

- Cursor Cloud Agents work on **GitHub repositories only**
- Provide a GitHub repo URL and branch when running a Cursor agent
- Get API key from [Cursor Dashboard → Integrations](https://cursor.com/dashboard?tab=integrations)

## Project Structure

```
Agent/
├── src/
│   ├── Agent.Api/        # .NET Web API + SignalR
│   ├── Agent.Core/       # Models, orchestration logic
│   └── Agent.Providers/  # Claude, OpenAI, Cursor adapters
├── client/               # Angular frontend
└── data/                 # Saved agents (created at runtime)
```
