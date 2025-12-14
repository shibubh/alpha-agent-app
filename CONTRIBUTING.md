# Contributing to Agent Orchestration System

Thank you for your interest in contributing to the Agent Orchestration System! This document provides guidelines and instructions for contributing.

## Development Setup

### Prerequisites
- .NET 10 SDK
- Git
- Your favorite IDE (Visual Studio, VS Code, or Rider)

### Getting Started

1. Fork the repository
2. Clone your fork:
   ```bash
   git clone https://github.com/YOUR_USERNAME/alpha-agent-app.git
   cd alpha-agent-app
   ```

3. Build the solution:
   ```bash
   dotnet build AgentOrchestration.sln
   ```

4. Create a new branch for your feature:
   ```bash
   git checkout -b feature/your-feature-name
   ```

## Project Structure

```
src/
├── AgentOrchestration.Core/       # Domain layer (models & interfaces)
├── AgentOrchestration.AI/         # AI provider implementations
├── AgentOrchestration.Agents/     # Business logic (agents)
└── AgentOrchestration.CLI/        # User interface (CLI)
```

## Coding Standards

### General Guidelines

1. **Follow C# Conventions**: Use standard C# naming and coding conventions
2. **Use Modern C# Features**: Leverage C# 12+ features (required properties, init-only setters, etc.)
3. **Enable Nullable Reference Types**: All projects have nullable enabled
4. **Async All the Way**: Use async/await for all I/O operations
5. **Dependency Injection**: Use constructor injection for dependencies

### Naming Conventions

- **Classes**: PascalCase (e.g., `PlanningAgent`)
- **Interfaces**: PascalCase with 'I' prefix (e.g., `IAIProvider`)
- **Methods**: PascalCase (e.g., `CreatePlanAsync`)
- **Properties**: PascalCase (e.g., `TaskStatus`)
- **Private Fields**: camelCase with underscore prefix (e.g., `_aiProvider`)
- **Parameters**: camelCase (e.g., `userPrompt`)

### Code Style

```csharp
// Good: Clear, descriptive names with XML documentation
/// <summary>
/// Creates an execution plan based on user requirements
/// </summary>
public async Task<ExecutionPlan> CreatePlanAsync(
    string userPrompt, 
    string techStack, 
    CancellationToken cancellationToken = default)
{
    // Implementation
}

// Good: Use required properties for mandatory data
public class TaskItem
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
}

// Good: Proper error handling
try
{
    var result = await _provider.SendRequestAsync(request);
    return result.Success ? result : throw new InvalidOperationException(result.ErrorMessage);
}
catch (Exception ex)
{
    // Log and handle appropriately
}
```

## Adding New Features

### Adding a New AI Provider

1. Create a new class in `AgentOrchestration.AI/Providers/`
2. Implement the `IAIProvider` interface
3. Add configuration section in appsettings.json
4. Update the provider factory in `Program.cs`

Example:
```csharp
namespace AgentOrchestration.AI.Providers;

public class GeminiProvider : IAIProvider
{
    public string ProviderName => "Gemini";
    
    public async Task<AIResponse> SendRequestAsync(
        AIRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Implement Gemini API integration
    }
}
```

### Adding a New Agent Type

1. Define interface in `AgentOrchestration.Core/Interfaces/`
2. Implement in `AgentOrchestration.Agents/`
3. Register in DI container
4. Use in the orchestrator

Example:
```csharp
// In Core/Interfaces/IValidationAgent.cs
public interface IValidationAgent
{
    Task<ValidationResult> ValidateAsync(ExecutionPlan plan);
}

// In Agents/ValidationAgent.cs
public class ValidationAgent : IValidationAgent
{
    public async Task<ValidationResult> ValidateAsync(ExecutionPlan plan)
    {
        // Implementation
    }
}
```

### Adding New Models

1. Add model class to `AgentOrchestration.Core/Models/`
2. Use `required` for mandatory properties
3. Use `init` for immutable properties
4. Add XML documentation

Example:
```csharp
namespace AgentOrchestration.Core.Models;

/// <summary>
/// Represents a validation result
/// </summary>
public class ValidationResult
{
    public required bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
}
```

## Testing Guidelines

While this initial version doesn't include tests, here's how to add them:

### Unit Tests

Create a new test project:
```bash
dotnet new xunit -n AgentOrchestration.Tests -f net10.0
dotnet sln add AgentOrchestration.Tests/AgentOrchestration.Tests.csproj
```

Example test:
```csharp
public class PlanningAgentTests
{
    [Fact]
    public async Task CreatePlanAsync_WithValidInput_ReturnsExecutionPlan()
    {
        // Arrange
        var mockProvider = new Mock<IAIProvider>();
        mockProvider.Setup(x => x.SendRequestAsync(It.IsAny<AIRequest>(), default))
            .ReturnsAsync(new AIResponse { Success = true, Content = "..." });
        
        var agent = new PlanningAgent(mockProvider.Object);
        
        // Act
        var plan = await agent.CreatePlanAsync("Build API", ".NET");
        
        // Assert
        Assert.NotNull(plan);
        Assert.NotEmpty(plan.Tasks);
    }
}
```

## Pull Request Process

1. **Create a Feature Branch**: Work on a descriptive branch name
   ```bash
   git checkout -b feature/add-gemini-provider
   ```

2. **Make Your Changes**: Follow the coding standards above

3. **Build and Test**: Ensure everything compiles
   ```bash
   dotnet build AgentOrchestration.sln
   ```

4. **Commit Your Changes**: Use clear, descriptive commit messages
   ```bash
   git commit -m "Add Gemini AI provider implementation"
   ```

5. **Push to Your Fork**:
   ```bash
   git push origin feature/add-gemini-provider
   ```

6. **Create Pull Request**: 
   - Go to GitHub and create a PR
   - Fill in the PR template
   - Link any related issues

### PR Checklist

- [ ] Code builds without errors
- [ ] All new code has XML documentation
- [ ] Follows existing code style and conventions
- [ ] No hardcoded credentials or sensitive data
- [ ] README updated if adding user-facing features
- [ ] Configuration examples updated if needed

## Documentation

### XML Documentation

Add XML comments to all public APIs:

```csharp
/// <summary>
/// Executes a task and returns the result
/// </summary>
/// <param name="task">The task to execute</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>The executed task with results</returns>
/// <exception cref="ArgumentNullException">Thrown when task is null</exception>
public async Task<TaskItem> ExecuteTaskAsync(
    TaskItem task, 
    CancellationToken cancellationToken = default)
{
    // Implementation
}
```

### Updating Documentation

If your changes affect:
- **User behavior**: Update README.md
- **Getting started**: Update QUICKSTART.md
- **System design**: Update ARCHITECTURE.md

## Common Tasks

### Adding a Configuration Setting

1. Add to `appsettings.json`:
   ```json
   {
     "MyFeature": {
       "Setting1": "value"
     }
   }
   ```

2. Read in `Program.cs`:
   ```csharp
   var mySetting = configuration["MyFeature:Setting1"];
   ```

### Adding a New Dependency

1. Add NuGet package:
   ```bash
   dotnet add src/ProjectName/ProjectName.csproj package PackageName
   ```

2. Update documentation if it's a user-facing change

### Debugging

1. Set environment variable:
   ```bash
   export OPENAI_API_KEY="your-test-key"
   ```

2. Run with debugger:
   ```bash
   cd src/AgentOrchestration.CLI
   dotnet run
   ```

## Code Review Guidelines

When reviewing PRs, check for:

1. **Correctness**: Does the code do what it claims?
2. **Testing**: Is there adequate test coverage?
3. **Documentation**: Are changes documented?
4. **Style**: Does it follow conventions?
5. **Performance**: Are there any obvious performance issues?
6. **Security**: Are there security concerns?

## Getting Help

- **Questions**: Open a GitHub issue with the "question" label
- **Bugs**: Open a GitHub issue with the "bug" label
- **Feature Requests**: Open a GitHub issue with the "enhancement" label
- **Discussion**: Use GitHub Discussions

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

## Recognition

Contributors will be recognized in the project README and release notes.

Thank you for contributing to the Agent Orchestration System!
