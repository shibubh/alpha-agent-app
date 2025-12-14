# Agent Orchestration System

A production-ready .NET 10 agent orchestration system featuring a planning agent that creates detailed planning documents describing what should be built (features, UI elements, content) without focusing on technical implementation. A separate Coding Agent can then use these plans to handle actual development.

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

- **Planning Agent**: Creates detailed planning documents focused on WHAT to build (features, UI, content) not HOW to build it
- **Multi-AI Support**: Choose between ChatGPT (OpenAI) or Claude (Anthropic) as the AI provider
- **Separation of Concerns**: Planning Agent handles planning, Coding Agent (separate) handles implementation
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

### JSON Output Mode

For automated workflows, you can use JSON output mode to get machine-readable results:

```bash
dotnet run --project src/AgentOrchestration.CLI/AgentOrchestration.CLI.csproj -- --json
```

Or use the short flag:

```bash
dotnet run --project src/AgentOrchestration.CLI/AgentOrchestration.CLI.csproj -- -j
```

In JSON mode:
- The agent outputs structured JSON instead of formatted console text
- Each task includes a `command` field with executable terminal commands when applicable
- The output can be parsed by other tools for automated execution
- No interactive prompts or colored output

### Example Session

```
╔═══════════════════════════════════════════════════════╗
║    Agent Orchestration System - Planning Agent       ║
╚═══════════════════════════════════════════════════════╝

🤖 Using AI Provider: ChatGPT

Please describe your task:
➤ Create a Fashion Industry Landing Page

═══════════════════════════════════════════════════════
📋 Creating Execution Plan...
═══════════════════════════════════════════════════════

📌 Goal: Create a Fashion Industry Landing Page
📄 Description: A comprehensive landing page showcasing fashion products and brand identity
📊 Total Planning Items: 5

1. Header Section
   Logo positioned on the left side, navigation menu in the center with links to Home, Collections, About, and Contact, shopping cart icon on the right

2. Hero Section
   Large fashion banner image showcasing latest collection, prominent headline "Elevate Your Style", subheading describing the brand's unique value proposition, call-to-action button "Shop Now"

3. Featured Collections Grid
   Four equal-sized blocks displaying different fashion categories: Women's Wear, Men's Wear, Accessories, and New Arrivals. Each block includes a representative image, category name, and brief description

4. Brand Story Section
   Centered content area with brand history, mission statement, and values. Include high-quality lifestyle image showing brand aesthetic

5. Footer Section
   Three columns: Company information and contact details on the left, quick links in the center, social media icons (Instagram, Facebook, Twitter) on the right, newsletter subscription form, copyright notice at bottom

✅ Detailed planning completed!

ℹ️  This detailed plan describes WHAT should be built.
ℹ️  Next step: Pass this plan to a Coding Agent to handle implementation.
```

### Example JSON Output

When using `--json` flag, the output is machine-readable:

```json
{
  "success": true,
  "plan": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "goal": "Create a Node.js Express REST API",
    "description": "A REST API with Express framework for handling CRUD operations",
    "techStack": "Not applicable - Planning phase",
    "status": "Created",
    "createdAt": "2024-12-14T02:00:00Z",
    "tasks": [
      {
        "id": "task-1",
        "title": "Initialize Node.js Project",
        "description": "Set up a new Node.js project with package.json",
        "preCommand": "node --version",
        "command": "npm init -y",
        "postCommand": "test -f package.json && echo 'package.json created successfully'",
        "order": 1,
        "status": "Pending"
      },
      {
        "id": "task-2",
        "title": "Install Express Framework",
        "description": "Install Express.js and required dependencies",
        "preCommand": null,
        "command": "npm install express",
        "postCommand": "npm list express",
        "order": 2,
        "status": "Pending"
      },
      {
        "id": "task-3",
        "title": "Install Development Dependencies",
        "description": "Install nodemon for development",
        "preCommand": null,
        "command": "npm install --save-dev nodemon",
        "postCommand": "npm list nodemon",
        "order": 3,
        "status": "Pending"
      },
      {
        "id": "task-4",
        "title": "Create API Endpoints",
        "description": "Define REST API endpoints for GET, POST, PUT, DELETE operations with proper routing and middleware",
        "preCommand": null,
        "command": null,
        "postCommand": null,
        "order": 4,
        "status": "Pending"
      },
      {
        "id": "task-5",
        "title": "Start Development Server",
        "description": "Run the Express server in development mode",
        "preCommand": "test -f server.js || echo 'Warning: server.js not found'",
        "command": "npm run dev",
        "postCommand": "curl -s http://localhost:3000/health",
        "order": 5,
        "status": "Pending"
      }
    ]
  }
}
```

Notice that:
- Tasks with installation or setup steps include executable `command` fields
- **preCommand**: Runs before the main command to check prerequisites or prepare the environment
- **postCommand**: Runs after the main command to verify success or test the result
- Descriptive tasks (like "Create API Endpoints") have `null` or empty commands
- The JSON can be parsed by automation tools to execute commands automatically in sequence (pre → command → post)

## 🏛️ System Components

### Planning Agent

The `PlanningAgent` is the core component that:
1. Takes user requirements as input
2. Communicates with the configured AI provider
3. Generates a detailed planning document describing WHAT should be built (features, UI elements, content)
4. Does NOT include development tasks, tech stack, or implementation details
5. Returns a structured `ExecutionPlan` object ready for a Coding Agent

### Task Executor (Optional)

The `TaskExecutor` (for backward compatibility):
1. Takes an execution plan as input
2. Can provide additional implementation guidance if requested
3. Note: This is separate from the Planning Agent's core purpose
4. A separate Coding Agent should handle actual implementation

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