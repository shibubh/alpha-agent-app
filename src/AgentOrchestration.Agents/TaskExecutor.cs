using AgentOrchestration.Core.Interfaces;
using AgentOrchestration.Core.Models;

namespace AgentOrchestration.Agents;

/// <summary>
/// Executes tasks from an execution plan
/// </summary>
public class TaskExecutor : ITaskExecutor
{
    private readonly IAIProvider _aiProvider;

    public TaskExecutor(IAIProvider aiProvider)
    {
        _aiProvider = aiProvider ?? throw new ArgumentNullException(nameof(aiProvider));
    }

    public async Task<TaskItem> ExecuteTaskAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));

        task.Status = Core.Models.TaskStatus.InProgress;
        task.StartedAt = DateTime.UtcNow;

        try
        {
            var systemMessage = @"You are a task execution agent. Your job is to provide detailed implementation guidance for the given task.

Provide:
1. Step-by-step implementation details
2. Code examples if applicable
3. Best practices and considerations
4. Expected outcomes

Be concise but thorough. Focus on actionable information.";

            var prompt = $@"Execute the following task:

**Task:** {task.Title}

**Description:** {task.Description}

Please provide detailed implementation guidance for completing this task.";

            var aiRequest = new AIRequest
            {
                Prompt = prompt,
                SystemMessage = systemMessage,
                Temperature = 0.7,
                MaxTokens = 1500
            };

            var response = await _aiProvider.SendRequestAsync(aiRequest, cancellationToken);

            if (!response.Success)
            {
                task.Status = Core.Models.TaskStatus.Failed;
                task.ErrorMessage = response.ErrorMessage;
                task.CompletedAt = DateTime.UtcNow;
                return task;
            }

            task.Result = response.Content;
            task.Status = Core.Models.TaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;

            return task;
        }
        catch (Exception ex)
        {
            task.Status = Core.Models.TaskStatus.Failed;
            task.ErrorMessage = $"Task execution failed: {ex.Message}";
            task.CompletedAt = DateTime.UtcNow;
            return task;
        }
    }

    public async Task<ExecutionPlan> ExecutePlanAsync(ExecutionPlan plan, CancellationToken cancellationToken = default)
    {
        if (plan == null)
            throw new ArgumentNullException(nameof(plan));

        plan.Status = PlanStatus.InProgress;
        plan.StartedAt = DateTime.UtcNow;

        try
        {
            var orderedTasks = plan.Tasks.OrderBy(t => t.Order).ToList();

            foreach (var task in orderedTasks)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    plan.Status = PlanStatus.Failed;
                    return plan;
                }

                await ExecuteTaskAsync(task, cancellationToken);

                // If a task fails, we continue with remaining tasks but mark the plan accordingly
                if (task.Status == Core.Models.TaskStatus.Failed)
                {
                    Console.WriteLine($"Warning: Task '{task.Title}' failed, but continuing with remaining tasks.");
                }
            }

            // Check if all tasks completed successfully
            var allCompleted = plan.Tasks.All(t => t.Status == Core.Models.TaskStatus.Completed);
            plan.Status = allCompleted ? PlanStatus.Completed : PlanStatus.Failed;
            plan.CompletedAt = DateTime.UtcNow;

            return plan;
        }
        catch (Exception ex)
        {
            plan.Status = PlanStatus.Failed;
            plan.CompletedAt = DateTime.UtcNow;
            throw new InvalidOperationException($"Plan execution failed: {ex.Message}", ex);
        }
    }
}
