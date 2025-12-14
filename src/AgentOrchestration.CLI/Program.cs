using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AgentOrchestration.Core.Interfaces;
using AgentOrchestration.AI.Providers;
using AgentOrchestration.Agents;
using AgentOrchestration.Core.Models;

namespace AgentOrchestration.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
        Console.WriteLine("║    Agent Orchestration System - Planning Agent       ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
        Console.WriteLine();

        try
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Setup dependency injection
            var serviceProvider = ConfigureServices(configuration);

            // Run the orchestration
            var orchestrator = serviceProvider.GetRequiredService<IOrchestrator>();
            await orchestrator.RunAsync();

            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ Fatal Error: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }

    private static ServiceProvider ConfigureServices(IConfiguration configuration)
    {
        var services = new ServiceCollection();

        // Register HttpClient
        services.AddHttpClient();

        // Register AI Provider based on configuration
        var providerType = configuration["AIProvider:Provider"] ?? "ChatGPT";

        services.AddSingleton<IAIProvider>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();

            if (providerType.Equals("Claude", StringComparison.OrdinalIgnoreCase))
            {
                var apiKey = configuration["AIProvider:Claude:ApiKey"]
                    ?? Environment.GetEnvironmentVariable("CLAUDE_API_KEY")
                    ?? throw new InvalidOperationException("Claude API key not configured");

                var model = configuration["AIProvider:Claude:Model"] ?? "claude-3-5-sonnet-20241022";
                return new ClaudeProvider(httpClient, apiKey, model);
            }
            else // Default to ChatGPT
            {
                var apiKey = configuration["AIProvider:ChatGPT:ApiKey"]
                    ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                    ?? throw new InvalidOperationException("OpenAI API key not configured");

                var model = configuration["AIProvider:ChatGPT:Model"] ?? "gpt-4";
                return new ChatGPTProvider(httpClient, apiKey, model);
            }
        });

        // Register agents and services
        services.AddSingleton<IPlanningAgent, PlanningAgent>();
        services.AddSingleton<ITaskExecutor, TaskExecutor>();
        services.AddSingleton<IOrchestrator, Orchestrator>();

        return services.BuildServiceProvider();
    }
}

/// <summary>
/// Main orchestrator interface
/// </summary>
public interface IOrchestrator
{
    Task RunAsync();
}

/// <summary>
/// Main orchestrator that coordinates the entire workflow
/// </summary>
public class Orchestrator : IOrchestrator
{
    private readonly IPlanningAgent _planningAgent;
    private readonly ITaskExecutor _taskExecutor;
    private readonly IAIProvider _aiProvider;

    public Orchestrator(
        IPlanningAgent planningAgent,
        ITaskExecutor taskExecutor,
        IAIProvider aiProvider)
    {
        _planningAgent = planningAgent ?? throw new ArgumentNullException(nameof(planningAgent));
        _taskExecutor = taskExecutor ?? throw new ArgumentNullException(nameof(taskExecutor));
        _aiProvider = aiProvider ?? throw new ArgumentNullException(nameof(aiProvider));
    }

    public async Task RunAsync()
    {
        Console.WriteLine($"🤖 Using AI Provider: {_aiProvider.ProviderName}");
        Console.WriteLine();

        // Get user input
        Console.WriteLine("Please describe your task:");
        Console.Write("➤ ");
        var userPrompt = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(userPrompt))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Task description cannot be empty!");
            Console.ResetColor();
            return;
        }

        Console.WriteLine("\nPlease specify your tech stack (e.g., .NET, Python, React, etc.):");
        Console.Write("➤ ");
        var techStack = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(techStack))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Tech stack cannot be empty!");
            Console.ResetColor();
            return;
        }

        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("📋 Creating Execution Plan...");
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine();

        // Create plan
        ExecutionPlan plan;
        try
        {
            plan = await _planningAgent.CreatePlanAsync(userPrompt, techStack);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Failed to create plan: {ex.Message}");
            Console.ResetColor();
            return;
        }

        // Display plan
        DisplayPlan(plan);

        // Ask for confirmation
        Console.WriteLine();
        Console.Write("Would you like to execute this plan? (yes/no): ");
        var confirmation = Console.ReadLine();

        if (!confirmation?.Equals("yes", StringComparison.OrdinalIgnoreCase) ?? true)
        {
            Console.WriteLine("❌ Execution cancelled by user.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("🚀 Executing Tasks...");
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine();

        // Execute plan
        await _taskExecutor.ExecutePlanAsync(plan);

        // Display results
        DisplayResults(plan);
    }

    private void DisplayPlan(ExecutionPlan plan)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"📌 Goal: {plan.Goal}");
        Console.WriteLine($"🔧 Tech Stack: {plan.TechStack}");
        Console.WriteLine($"📄 Description: {plan.Description}");
        Console.WriteLine($"📊 Total Tasks: {plan.Tasks.Count}");
        Console.ResetColor();
        Console.WriteLine();

        foreach (var task in plan.Tasks.OrderBy(t => t.Order))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Task {task.Order}: {task.Title}");
            Console.ResetColor();
            Console.WriteLine($"  └─ {task.Description}");
            Console.WriteLine();
        }
    }

    private void DisplayResults(ExecutionPlan plan)
    {
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("📊 Execution Results");
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine();

        var completedCount = plan.Tasks.Count(t => t.Status == Core.Models.TaskStatus.Completed);
        var failedCount = plan.Tasks.Count(t => t.Status == Core.Models.TaskStatus.Failed);

        foreach (var task in plan.Tasks.OrderBy(t => t.Order))
        {
            var statusIcon = task.Status switch
            {
                Core.Models.TaskStatus.Completed => "✅",
                Core.Models.TaskStatus.Failed => "❌",
                Core.Models.TaskStatus.InProgress => "🔄",
                _ => "⏸️"
            };

            Console.ForegroundColor = task.Status == Core.Models.TaskStatus.Completed ? ConsoleColor.Green :
                                    task.Status == Core.Models.TaskStatus.Failed ? ConsoleColor.Red :
                                    ConsoleColor.Yellow;

            Console.WriteLine($"{statusIcon} Task {task.Order}: {task.Title}");
            Console.ResetColor();

            if (!string.IsNullOrEmpty(task.Result))
            {
                Console.WriteLine($"  Result:");
                var resultLines = task.Result.Split('\n');
                foreach (var line in resultLines.Take(5)) // Show first 5 lines
                {
                    Console.WriteLine($"    {line}");
                }
                if (resultLines.Length > 5)
                {
                    Console.WriteLine($"    ... ({resultLines.Length - 5} more lines)");
                }
            }

            if (!string.IsNullOrEmpty(task.ErrorMessage))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  Error: {task.ErrorMessage}");
                Console.ResetColor();
            }

            Console.WriteLine();
        }

        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"📈 Summary: {completedCount} completed, {failedCount} failed out of {plan.Tasks.Count} total tasks");
        Console.ResetColor();

        if (plan.Status == PlanStatus.Completed)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Plan execution completed successfully!");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠️  Plan execution completed with some failures.");
        }
        Console.ResetColor();

        var duration = plan.CompletedAt.HasValue && plan.StartedAt.HasValue
            ? (plan.CompletedAt.Value - plan.StartedAt.Value).TotalSeconds
            : 0;

        Console.WriteLine($"⏱️  Total execution time: {duration:F2} seconds");
        Console.WriteLine();
    }
}
