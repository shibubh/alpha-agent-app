using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AgentOrchestration.Core.Interfaces;
using AgentOrchestration.AI.Providers;
using AgentOrchestration.Agents;
using AgentOrchestration.Core.Models;

namespace AgentOrchestration.CLI;

internal static class JsonConfiguration
{
    public static readonly System.Text.Json.JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
    };
}

class Program
{

    static async Task<int> Main(string[] args)
    {
        // Check for JSON output flag
        bool jsonOutput = args.Contains("--json") || args.Contains("-j");
        
        if (!jsonOutput)
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
            Console.WriteLine("║    Agent Orchestration System - Planning Agent       ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
            Console.WriteLine();
        }

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
            await orchestrator.RunAsync(jsonOutput);

            return 0;
        }
        catch (Exception ex)
        {
            if (jsonOutput)
            {
                var errorOutput = new
                {
                    success = false,
                    error = ex.Message
                };
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(errorOutput, JsonConfiguration.Options));
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Fatal Error: {ex.Message}");
                Console.ResetColor();
            }
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
    Task RunAsync(bool jsonOutput = false);
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

    public async Task RunAsync(bool jsonOutput = false)
    {
        if (!jsonOutput)
        {
            Console.WriteLine($"🤖 Using AI Provider: {_aiProvider.ProviderName}");
            Console.WriteLine();

            // Get user input
            Console.WriteLine("Please describe your task:");
            Console.Write("➤ ");
        }
        
        var userPrompt = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(userPrompt))
        {
            if (jsonOutput)
            {
                var errorOutput = new { success = false, error = "Task description cannot be empty" };
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(errorOutput, JsonConfiguration.Options));
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Task description cannot be empty!");
                Console.ResetColor();
            }
            return;
        }

        // Tech stack is no longer required for planning agent
        // Planning agent focuses on WHAT to build, not HOW

        if (!jsonOutput)
        {
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("📋 Creating Execution Plan...");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine();
        }

        // Create plan
        ExecutionPlan plan;
        try
        {
            plan = await _planningAgent.CreatePlanAsync(userPrompt);
        }
        catch (Exception ex)
        {
            if (jsonOutput)
            {
                var errorOutput = new { success = false, error = $"Failed to create plan: {ex.Message}" };
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(errorOutput, JsonConfiguration.Options));
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Failed to create plan: {ex.Message}");
                Console.ResetColor();
            }
            return;
        }

        if (jsonOutput)
        {
            // Output as JSON
            DisplayPlanJson(plan);
        }
        else
        {
            // Display plan
            DisplayPlan(plan);

            // Planning is complete - inform user about next steps
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Detailed planning completed!");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("ℹ️  This detailed plan describes WHAT should be built.");
            Console.WriteLine("ℹ️  Next step: Pass this plan to a Coding Agent to handle implementation.");
            Console.WriteLine();
            
            // Ask if user wants to proceed with execution (kept for backward compatibility)
            Console.Write("Would you like to proceed with execution guidance? (yes/no): ");
            var confirmation = Console.ReadLine();

            if (confirmation?.Equals("yes", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                Console.WriteLine();
                Console.WriteLine("═══════════════════════════════════════════════════════");
                Console.WriteLine("🚀 Generating Implementation Guidance...");
                Console.WriteLine("═══════════════════════════════════════════════════════");
                Console.WriteLine();

                // Execute plan
                await _taskExecutor.ExecutePlanAsync(plan);

                // Display results
                DisplayResults(plan);
            }
            else
            {
                Console.WriteLine("✅ Planning phase completed. Plan is ready for the Coding Agent.");
            }
        }
    }

    private void DisplayPlan(ExecutionPlan plan)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"📌 Goal: {plan.Goal}");
        Console.WriteLine($"📄 Description: {plan.Description}");
        Console.WriteLine($"📊 Total Planning Items: {plan.Tasks.Count}");
        Console.ResetColor();
        Console.WriteLine();

        foreach (var task in plan.Tasks.OrderBy(t => t.Order))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{task.Order}. {task.Title}");
            Console.ResetColor();
            Console.WriteLine($"   {task.Description}");
            if (task.Commands != null && task.Commands.Any())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"   💻 Commands:");
                foreach (var cmd in task.Commands)
                {
                    Console.WriteLine($"      - {cmd}");
                }
                Console.ResetColor();
            }
            if (!string.IsNullOrWhiteSpace(task.PostCommand))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"   ✅ Post-Command: {task.PostCommand}");
                Console.ResetColor();
            }
            Console.WriteLine();
        }
    }

    private void DisplayPlanJson(ExecutionPlan plan)
    {
        var output = new
        {
            success = true,
            plan = new
            {
                id = plan.Id,
                goal = plan.Goal,
                description = plan.Description,
                techStack = plan.TechStack,
                status = plan.Status.ToString(),
                createdAt = plan.CreatedAt,
                tasks = plan.Tasks.OrderBy(t => t.Order).Select(t => new
                {
                    id = t.Id,
                    title = t.Title,
                    description = t.Description,
                    commands = t.Commands,
                    postCommand = t.PostCommand,
                    order = t.Order,
                    status = t.Status.ToString()
                }).ToList()
            }
        };

        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(output, JsonConfiguration.Options));
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
