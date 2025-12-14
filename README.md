# Agent Orchestration System

A production-ready .NET 10 agent orchestration system featuring a planning agent that breaks down user tasks into detailed execution plans and executes them sequentially using AI providers (ChatGPT or Claude).

## 🏗️ Architecture

The system is organized into multiple projects following clean architecture principles:

```
AgentOrchestration/
├── src/
│   ├── AgentOrchestration.Core/       # Core domain models and interfaces
│   ├── AgentOrchestration.AI/         # AI provider implementations
│   ├── AgentOrchestration.Agents/     # Agent implementations
│   └── AgentOrchestration.CLI/        # Command-line interface
```

### Projects

- **AgentOrchestration.Core**: Contains core domain models (`TaskItem`, `ExecutionPlan`, `AIRequest`, `AIResponse`) and interfaces (`IAIProvider`, `IPlanningAgent`, `ITaskExecutor`)
- **AgentOrchestration.AI**: Implements AI provider integrations for ChatGPT and Claude
- **AgentOrchestration.Agents**: Contains the `PlanningAgent` and `TaskExecutor` implementations
- **AgentOrchestration.CLI**: Console application providing user interaction

## ✨ Features

- **Planning Agent**: Analyzes user requirements and tech stack to create detailed execution plans
- **Multi-AI Support**: Choose between ChatGPT (OpenAI) or Claude (Anthropic) as the AI provider
- **Sequential Task Execution**: Executes tasks one by one with detailed progress tracking
- **Production-Ready**: Proper error handling, dependency injection, configuration management
- **Clean Architecture**: Separation of concerns with multiple projects
- **User-Friendly CLI**: Interactive command-line interface with colored output

## 🚀 Getting Started

### Prerequisites

- .NET 10 SDK
- API key for either OpenAI (ChatGPT) or Anthropic (Claude)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/shibubh/alpha-agent-app.git
cd alpha-agent-app
```

2. Build the solution:
```bash
dotnet build AgentOrchestration.sln
```

### Configuration

You can configure the AI provider in two ways:

#### Option 1: Using appsettings.json

Edit `src/AgentOrchestration.CLI/appsettings.json`:

```json
{
  "AIProvider": {
    "Provider": "ChatGPT",  // or "Claude"
    "ChatGPT": {
      "ApiKey": "your-openai-api-key-here",
      "Model": "gpt-4"
    },
    "Claude": {
      "ApiKey": "your-claude-api-key-here",
      "Model": "claude-3-5-sonnet-20241022"
    }
  }
}
```

#### Option 2: Using Environment Variables

Set environment variables (recommended for production):

For ChatGPT:
```bash
export OPENAI_API_KEY="your-openai-api-key-here"
```

For Claude:
```bash
export CLAUDE_API_KEY="your-claude-api-key-here"
```

Then update the `Provider` in `appsettings.json` to match your choice.

## 📖 Usage

### Running the CLI

Navigate to the CLI project directory and run:

```bash
cd src/AgentOrchestration.CLI
dotnet run
```

Or build and run from the root:

```bash
dotnet build AgentOrchestration.sln
dotnet run --project src/AgentOrchestration.CLI/AgentOrchestration.CLI.csproj
```

### Example Session

```
╔═══════════════════════════════════════════════════════╗
║    Agent Orchestration System - Planning Agent       ║
╚═══════════════════════════════════════════════════════╝

🤖 Using AI Provider: ChatGPT

Please describe your task:
➤ Create a REST API for a blog system with posts and comments

Please specify your tech stack (e.g., .NET, Python, React, etc.):
➤ .NET 10, ASP.NET Core, Entity Framework Core, PostgreSQL

═══════════════════════════════════════════════════════
📋 Creating Execution Plan...
═══════════════════════════════════════════════════════

📌 Goal: Create a REST API for a blog system with posts and comments
🔧 Tech Stack: .NET 10, ASP.NET Core, Entity Framework Core, PostgreSQL
📄 Description: Build a comprehensive REST API for blog management
📊 Total Tasks: 5

Task 1: Setup Project Structure
  └─ Create ASP.NET Core Web API project with proper folder structure

Task 2: Design Database Models
  └─ Create entity models for Posts and Comments with relationships

Task 3: Configure Entity Framework Core
  └─ Setup DbContext and PostgreSQL connection

Task 4: Implement API Controllers
  └─ Create controllers with CRUD operations for Posts and Comments

Task 5: Add Validation and Error Handling
  └─ Implement input validation and global error handling

Would you like to execute this plan? (yes/no): yes

═══════════════════════════════════════════════════════
🚀 Executing Tasks...
═══════════════════════════════════════════════════════

✅ Task 1: Setup Project Structure
✅ Task 2: Design Database Models
✅ Task 3: Configure Entity Framework Core
✅ Task 4: Implement API Controllers
✅ Task 5: Add Validation and Error Handling

═══════════════════════════════════════════════════════
📈 Summary: 5 completed, 0 failed out of 5 total tasks
✅ Plan execution completed successfully!
⏱️  Total execution time: 45.32 seconds
```

## 🏛️ System Components

### Planning Agent

The `PlanningAgent` is the core component that:
1. Takes user requirements and tech stack as input
2. Communicates with the configured AI provider
3. Generates a detailed execution plan with numbered tasks
4. Returns a structured `ExecutionPlan` object

### Task Executor

The `TaskExecutor`:
1. Takes an execution plan as input
2. Executes tasks sequentially in order
3. Provides implementation guidance for each task using the AI provider
4. Tracks status and results for each task
5. Continues execution even if individual tasks fail

### AI Providers

Two AI provider implementations are available:

- **ChatGPTProvider**: Integrates with OpenAI's GPT-4
- **ClaudeProvider**: Integrates with Anthropic's Claude-3.5-Sonnet

Both implement the `IAIProvider` interface for consistent behavior.

## 🔧 Development

### Building

```bash
dotnet build AgentOrchestration.sln
```

### Project Structure

```
AgentOrchestration.Core/
├── Models/
│   ├── TaskItem.cs          # Task model with status tracking
│   ├── ExecutionPlan.cs     # Plan model with tasks collection
│   └── AIRequest.cs         # AI request/response models
└── Interfaces/
    ├── IAIProvider.cs       # AI provider interface
    ├── IPlanningAgent.cs    # Planning agent interface
    └── ITaskExecutor.cs     # Task executor interface

AgentOrchestration.AI/
└── Providers/
    ├── ChatGPTProvider.cs   # OpenAI implementation
    └── ClaudeProvider.cs    # Anthropic implementation

AgentOrchestration.Agents/
├── PlanningAgent.cs         # Planning agent implementation
└── TaskExecutor.cs          # Task executor implementation

AgentOrchestration.CLI/
├── Program.cs               # CLI application entry point
└── appsettings.json         # Configuration file
```

## 🛠️ Technology Stack

- **.NET 10**: Latest .NET framework
- **Microsoft.Extensions.DependencyInjection**: Dependency injection
- **Microsoft.Extensions.Configuration**: Configuration management
- **Microsoft.Extensions.Http**: HTTP client factory
- **System.Text.Json**: JSON serialization

## 📝 License

This project is open source and available under the MIT License.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📧 Contact

For questions or support, please open an issue on GitHub.