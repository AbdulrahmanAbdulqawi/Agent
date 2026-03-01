# App Improvement Plan

This document outlines a comprehensive plan to improve the Autonomous AI Coding Agent application.

---

## Executive Summary

The app is a well-structured Angular + .NET 8 web application for running autonomous AI coding agents. While the core architecture is solid with good separation of concerns and a clean provider abstraction, there are significant opportunities for improvement in testing, security, UX, reliability, and scalability.

---

## Priority Levels

- **P0 - Critical**: Security and data integrity issues
- **P1 - High**: Core functionality and reliability improvements
- **P2 - Medium**: Developer experience and maintainability
- **P3 - Low**: Nice-to-have enhancements

---

## 1. Testing (P1)

### Current State
- No unit tests or integration tests
- Angular schematics configured with `skipTests: true`
- No test coverage reporting

### Improvements

#### Backend (.NET)

| Task | Description |
|------|-------------|
| Add xUnit test project | Create `Agent.Tests` project with xUnit, Moq, and FluentAssertions |
| Unit test `CompletionDetection` | Test `IsComplete()` and `IsDoomLoop()` with various inputs |
| Unit test `PromptBuilder` | Test prompt generation with different `ExtraInfo` combinations |
| Unit test providers | Mock `HttpClient` to test Claude, OpenAI, and Cursor provider logic |
| Integration tests | Test API endpoints with `WebApplicationFactory` |
| Add test coverage | Configure `coverlet` for code coverage reporting |

#### Frontend (Angular)

| Task | Description |
|------|-------------|
| Enable component tests | Remove `skipTests: true` from angular.json schematics |
| Service tests | Test `AgentService` API calls and SignalR connection |
| Component tests | Test form validation, user interactions, state changes |
| E2E tests | Add Playwright or Cypress for end-to-end testing |

---

## 2. Security (P0)

### Current State
- No authentication or authorization
- API keys stored in config files
- No rate limiting
- No input validation on API endpoints

### Improvements

| Priority | Task | Description |
|----------|------|-------------|
| P0 | Add authentication | Implement JWT-based auth or integrate with OAuth providers |
| P0 | Secure API keys | Move to Azure Key Vault, AWS Secrets Manager, or encrypted storage |
| P0 | Input validation | Add FluentValidation for all request DTOs |
| P0 | Rate limiting | Add `AspNetCoreRateLimit` to prevent API abuse |
| P1 | HTTPS enforcement | Configure HTTPS redirect and HSTS |
| P1 | CORS hardening | Make CORS origins configurable per environment |
| P1 | Audit logging | Log all agent operations with user context |

---

## 3. Persistence & Data Management (P1)

### Current State
- Single JSON file (`agents.json`) for persistence
- No backup or migration strategy
- Completed runs stored in memory only (lost on restart)
- No concurrency handling for file operations

### Improvements

| Task | Description |
|------|-------------|
| Add SQLite/PostgreSQL | Replace JSON file with proper database using EF Core |
| Run history persistence | Store completed sessions in database |
| Add migrations | Use EF Core migrations for schema versioning |
| Implement optimistic concurrency | Add concurrency tokens to prevent data conflicts |
| Add data export | Allow exporting agent configs and run history |
| Backup strategy | Implement automated backup for database |

### Database Schema Proposal

```
Agents
├── Id (GUID, PK)
├── Name
├── Provider
├── WorkspacePath
├── CreatedAt
└── UpdatedAt

AgentSessions
├── Id (GUID, PK)
├── AgentId (FK)
├── RunId
├── Status
├── Goal
├── Summary
├── Error
├── StartedAt
├── CompletedAt
└── ExtraInfo (JSON)

ConversationMessages
├── Id (GUID, PK)
├── SessionId (FK)
├── Type
├── Text
├── Timestamp
└── Sequence
```

---

## 4. Error Handling & Resilience (P1)

### Current State
- Some catch blocks swallow errors silently
- `AgentStore` returns empty dict on JSON parse failure
- No retry logic for API calls
- No circuit breaker pattern

### Improvements

| Task | Description |
|------|-------------|
| Global exception handler | Add `IExceptionHandler` middleware |
| Structured logging | Add Serilog with structured JSON logs |
| Retry policies | Add Polly for transient fault handling |
| Circuit breaker | Implement circuit breaker for external API calls |
| Health checks | Add health check endpoints for monitoring |
| Graceful degradation | Handle provider outages gracefully |

### Example Polly Configuration

```csharp
services.AddHttpClient<ClaudeProvider>()
    .AddTransientHttpErrorPolicy(p => 
        p.WaitAndRetryAsync(3, attempt => 
            TimeSpan.FromSeconds(Math.Pow(2, attempt))))
    .AddTransientHttpErrorPolicy(p => 
        p.CircuitBreakerAsync(5, TimeSpan.FromMinutes(1)));
```

---

## 5. Frontend UX Improvements (P2)

### Current State
- Generic page title ("Client")
- No loading states or skeleton screens
- Basic error display
- No confirmation dialogs for destructive actions
- No keyboard shortcuts

### Improvements

| Task | Description |
|------|-------------|
| Dynamic page titles | Set meaningful titles per view |
| Loading states | Add skeleton loaders and spinners |
| Toast notifications | Show success/error toasts for operations |
| Confirmation dialogs | Add confirm dialog for delete operations |
| Form improvements | Better validation messages, auto-save drafts |
| Keyboard shortcuts | Add shortcuts (Ctrl+N new, Ctrl+S save, etc.) |
| Dark mode | Add dark/light theme toggle |
| Responsive design | Improve mobile/tablet layout |
| Agent run history | Show past runs per agent |
| Search & filter | Filter agents by name, provider, status |

### UI Component Additions

- Toast/Snackbar component
- Confirmation dialog component
- Loading skeleton component
- Empty state component
- Pagination component (for run history)

---

## 6. Configuration & Flexibility (P2)

### Current State
- Hardcoded model names (`claude-sonnet-4-20250514`, `gpt-4o`)
- Fixed iteration limit (20) and timeout (30 min)
- No way to customize completion signals

### Improvements

| Task | Description |
|------|-------------|
| Configurable models | Allow model selection per agent/run |
| Configurable limits | Make iteration limit and timeout configurable |
| Custom completion signals | Allow custom completion patterns |
| System prompt templates | Allow customizable system prompts |
| Environment-based config | Proper appsettings.{Environment}.json |
| Feature flags | Add feature flag system for gradual rollouts |

### Configuration Example

```json
{
  "Providers": {
    "Claude": {
      "ApiKey": "...",
      "DefaultModel": "claude-sonnet-4-20250514",
      "AvailableModels": ["claude-sonnet-4-20250514", "claude-3-5-haiku-20241022"]
    },
    "Orchestration": {
      "MaxIterations": 20,
      "TimeoutMinutes": 30,
      "CompletionSignals": ["<DONE>", "[COMPLETE]", "[DONE]"]
    }
  }
}
```

---

## 7. Code Quality & Maintainability (P2)

### Current State
- No linting rules enforced
- Limited inline documentation
- No architecture documentation

### Improvements

| Task | Description |
|------|-------------|
| Add ESLint | Configure ESLint with Angular rules |
| Add Prettier | Consistent code formatting |
| Add .editorconfig | Cross-IDE formatting consistency |
| XML documentation | Add XML docs to public APIs |
| Architecture docs | Document system architecture with diagrams |
| API documentation | Auto-generate API docs from Swagger/OpenAPI |
| Code review checklist | Create PR checklist for consistency |

---

## 8. Performance & Scalability (P3)

### Current State
- All runs managed in-memory
- No caching layer
- Single-instance design

### Improvements

| Task | Description |
|------|-------------|
| Response caching | Cache agent list and static data |
| SignalR scale-out | Add Redis backplane for multi-instance |
| Background job queue | Replace in-memory runs with Hangfire/Azure Queue |
| Connection pooling | Configure HTTP client connection pooling |
| Memory optimization | Limit in-memory conversation history |
| Metrics & monitoring | Add Prometheus metrics or Application Insights |

---

## 9. DevOps & Deployment (P2)

### Current State
- Local development only
- No CI/CD pipeline
- No containerization

### Improvements

| Task | Description |
|------|-------------|
| Dockerfile | Create multi-stage Dockerfile |
| docker-compose | Local development with all dependencies |
| CI pipeline | GitHub Actions for build, test, lint |
| CD pipeline | Automated deployment to staging/production |
| Environment configs | Separate configs per environment |
| Infrastructure as Code | Terraform/Pulumi for cloud resources |

### Example GitHub Actions Workflow

```yaml
name: CI
on: [push, pull_request]
jobs:
  backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x
      - run: dotnet build
      - run: dotnet test --collect:"XPlat Code Coverage"
      
  frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: 20
      - run: npm ci
        working-directory: client
      - run: npm run lint
        working-directory: client
      - run: npm test -- --no-watch --no-progress
        working-directory: client
```

---

## 10. Feature Enhancements (P3)

### New Capabilities

| Feature | Description |
|---------|-------------|
| Agent templates | Pre-configured agents for common tasks |
| Scheduled runs | Run agents on a schedule (cron) |
| Webhooks | Notify external systems on run completion |
| Multi-agent workflows | Chain multiple agents together |
| Run comparison | Compare outputs across different providers |
| Cost tracking | Track API usage and costs per run |
| Conversation export | Export runs to Markdown/JSON/PDF |
| Agent sharing | Export/import agent configurations |
| Team collaboration | Multi-user support with shared agents |

---

## Implementation Roadmap

### Phase 1: Foundation (Critical)
1. Add authentication and authorization
2. Implement input validation
3. Add database persistence (SQLite for MVP)
4. Set up basic logging with Serilog
5. Add health check endpoints

### Phase 2: Reliability
1. Add unit tests for core logic
2. Implement retry policies with Polly
3. Add global exception handling
4. Improve error messages in UI
5. Add loading states and toast notifications

### Phase 3: Developer Experience
1. Set up CI/CD pipeline
2. Add Dockerfile and docker-compose
3. Configure ESLint and Prettier
4. Add integration tests
5. Generate API documentation

### Phase 4: Scale & Polish
1. Migrate to PostgreSQL if needed
2. Add Redis for SignalR scale-out
3. Implement dark mode
4. Add agent templates
5. Add cost tracking

---

## Quick Wins

These can be implemented quickly with high impact:

1. **Add page title** - Update `index.html` title
2. **Add loading spinner** - Show during API calls
3. **Add confirmation dialog** - Before deleting agents
4. **Add toast notifications** - For success/error feedback
5. **Fix TypeScript types** - Replace `{}` with proper interfaces
6. **Add health endpoint** - Simple `/health` endpoint
7. **Add request logging** - Basic HTTP request logging
8. **Add `.editorconfig`** - Consistent formatting

---

## Conclusion

This improvement plan provides a structured approach to enhancing the application from its current solid foundation to a production-ready, maintainable, and scalable system. Priorities can be adjusted based on specific needs and constraints.
