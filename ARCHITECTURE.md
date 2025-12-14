# Architecture Documentation

## Overview

The Agent Orchestration System is built using clean architecture principles with clear separation of concerns across multiple projects.

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         CLI Layer                           │
│  - User Interaction                                         │
│  - Configuration Management                                 │
│  - Dependency Injection Setup                               │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────────┐
│                      Agents Layer                           │
│  ┌──────────────────┐         ┌──────────────────┐         │
│  │ Planning Agent   │         │  Task Executor   │         │
│  │  - Parse input   │         │  - Execute tasks │         │
│  │  - Create plan   │         │  - Track status  │         │
│  └──────────────────┘         └──────────────────┘         │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────────┐
│                        AI Layer                             │
│  ┌──────────────────┐         ┌──────────────────┐         │
│  │ ChatGPT Provider │         │  Claude Provider │         │
│  │  - OpenAI API    │         │  - Anthropic API │         │
│  └──────────────────┘         └──────────────────┘         │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────────┐
│                      Core Layer                             │
│  - Domain Models (TaskItem, ExecutionPlan)                 │
│  - Interfaces (IAIProvider, IPlanningAgent, ITaskExecutor) │
│  - Business Logic Contracts                                 │
└─────────────────────────────────────────────────────────────┘
```

## Project Structure

### 1. AgentOrchestration.Core (Domain Layer)

**Purpose**: Contains core domain models and interfaces that define the business logic contracts.

**Key Components**:
- `Models/TaskItem.cs`: Represents a single executable task
- `Models/ExecutionPlan.cs`: Contains a collection of tasks to be executed
- `Models/AIRequest.cs` & `AIResponse.cs`: Data transfer objects for AI communication
- `Interfaces/IAIProvider.cs`: Contract for AI provider implementations
- `Interfaces/IPlanningAgent.cs`: Contract for planning agent
- `Interfaces/ITaskExecutor.cs`: Contract for task execution

**Dependencies**: None (pure domain layer)

### 2. AgentOrchestration.AI (Infrastructure Layer)

**Purpose**: Implements AI provider integrations for external services.

**Key Components**:
- `Providers/ChatGPTProvider.cs`: OpenAI GPT-4 implementation
- `Providers/ClaudeProvider.cs`: Anthropic Claude implementation

**Design Patterns**:
- **Strategy Pattern**: Different AI providers implement same interface
- **Factory Pattern**: HttpClient factory for proper resource management

**Dependencies**: 
- AgentOrchestration.Core
- System.Net.Http
- System.Text.Json

### 3. AgentOrchestration.Agents (Application Layer)

**Purpose**: Contains the business logic for planning and task execution.

**Key Components**:

#### PlanningAgent
- Communicates with AI providers
- Parses AI responses into structured plans
- Handles JSON extraction from markdown code blocks
- Creates detailed planning documents focused on WHAT to build
- Does NOT include development tasks or tech stack considerations

**Workflow**:
1. Receives user prompt
2. Constructs detailed prompt for AI focused on product features and design
3. Sends request to AI provider
4. Parses JSON response
5. Creates ExecutionPlan with detailed planning items describing features, UI, and content

#### TaskExecutor (Optional - for backward compatibility)
- Can provide additional implementation guidance
- Note: Separate from Planning Agent's core purpose
- A dedicated Coding Agent should handle actual implementation

**Workflow**:
1. Receives ExecutionPlan
2. Can optionally provide implementation guidance
3. Planning phase is complete before this step
4. Implementation should be handled by a separate Coding Agent

**Dependencies**:
- AgentOrchestration.Core
- AgentOrchestration.AI

### 4. AgentOrchestration.CLI (Presentation Layer)

**Purpose**: Provides user interface and application entry point.

**Key Components**:

#### Program.cs
- Entry point with Main method
- Configuration setup (appsettings.json + environment variables)
- Dependency injection container configuration
- AI provider selection logic

#### Orchestrator
- Coordinates the overall workflow
- Manages user interaction
- Displays formatted output
- Handles confirmation flow

**Workflow**:
1. Display welcome banner
2. Get user input (task + tech stack)
3. Create execution plan via PlanningAgent
4. Display plan to user
5. Request confirmation
6. Execute plan via TaskExecutor
7. Display results and summary

**Dependencies**:
- AgentOrchestration.Core
- AgentOrchestration.Agents
- AgentOrchestration.AI
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Http

## Design Patterns

### 1. Dependency Injection
All dependencies are injected via constructor, promoting:
- Testability
- Loose coupling
- Maintainability

### 2. Interface Segregation
Clear interfaces define contracts:
- `IAIProvider`: AI communication
- `IPlanningAgent`: Plan creation
- `ITaskExecutor`: Task execution

### 3. Strategy Pattern
Multiple AI providers (ChatGPT, Claude) implement `IAIProvider` interface, allowing runtime provider selection.

### 4. Factory Pattern
HttpClient factory ensures proper resource management and allows configuration of HTTP clients.

## Data Flow

```
User Input (Prompt)
    ↓
Planning Agent
    ↓
AI Provider (ChatGPT/Claude)
    ↓
Detailed Planning Document
(Features, UI, Content - WHAT to build)
    ↓
Planning Complete
    ↓
[Next: Pass to Coding Agent for implementation]
```

## Configuration Management

Configuration is loaded from multiple sources (in priority order):
1. Environment Variables (highest priority)
2. appsettings.json
3. Default values in code

**Key Configuration**:
- `AIProvider:Provider`: "ChatGPT" or "Claude"
- `AIProvider:ChatGPT:ApiKey`: OpenAI API key
- `AIProvider:ChatGPT:Model`: Model name (default: "gpt-4")
- `AIProvider:Claude:ApiKey`: Anthropic API key
- `AIProvider:Claude:Model`: Model name (default: "claude-3-5-sonnet-20241022")

**Environment Variables**:
- `OPENAI_API_KEY`: Overrides ChatGPT API key
- `CLAUDE_API_KEY`: Overrides Claude API key

## Error Handling

### Levels of Error Handling

1. **AI Provider Level**
   - Catches HTTP errors
   - Returns AIResponse with Success=false
   - Includes error message

2. **Agent Level**
   - Validates inputs
   - Handles invalid AI responses
   - Continues execution on task failures

3. **CLI Level**
   - Displays user-friendly error messages
   - Uses colored console output
   - Provides graceful degradation

## Extensibility

### Adding a New AI Provider

1. Create new class implementing `IAIProvider`
2. Implement `SendRequestAsync` method
3. Add configuration in appsettings.json
4. Update DI registration in Program.cs

Example:
```csharp
public class GeminiProvider : IAIProvider
{
    public string ProviderName => "Gemini";
    
    public async Task<AIResponse> SendRequestAsync(
        AIRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Implementation
    }
}
```

### Adding a New Agent

1. Define interface in Core project
2. Implement in Agents project
3. Register in DI container
4. Use in Orchestrator

## Best Practices Implemented

1. **Async/Await Throughout**: All I/O operations are async
2. **CancellationToken Support**: Proper cancellation handling
3. **Separation of Concerns**: Each project has single responsibility
4. **Immutability**: Use of `required` and `init` for models
5. **Null Safety**: Nullable reference types enabled
6. **Resource Management**: Proper disposal via DI lifetime management
7. **Configuration Abstraction**: Configuration separated from code
8. **Logging Ready**: Console.WriteLine can be replaced with ILogger

## Testing Strategy

While tests are not included in this implementation, the architecture supports:

1. **Unit Tests**: Mock IAIProvider for agent testing
2. **Integration Tests**: Test with actual AI providers
3. **CLI Tests**: Test orchestration workflow
4. **Contract Tests**: Verify interface implementations

## Performance Considerations

1. **HttpClient Reuse**: HttpClientFactory prevents socket exhaustion
2. **Sequential Execution**: Tasks run one at a time (by design)
3. **Memory Efficiency**: Streaming responses from AI providers
4. **Cancellation Support**: Can cancel long-running operations

## Security Considerations

1. **API Key Protection**: Keys in environment variables, not in code
2. **Input Validation**: User inputs validated before processing
3. **Error Message Sanitization**: Sensitive data not exposed in errors
4. **HTTPS Only**: All AI provider communication over HTTPS

## Future Enhancements

Potential areas for extension:
1. Parallel task execution for independent tasks
2. Task dependency management
3. Persistent storage for plans and results
4. Web API interface in addition to CLI
5. Real-time progress updates via SignalR
6. Task rollback capabilities
7. Comprehensive logging with ILogger
8. Metrics and telemetry
